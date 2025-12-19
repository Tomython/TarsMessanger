using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TarsMessanger.Data;
using TarsMessanger.Services;

namespace TarsMessanger.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessengerService _messengerService;
    private readonly MessengerDbContext _context;
    private static readonly Dictionary<string, int> _connectionToUserId = new();  // connId → userId

    public ChatHub(IMessengerService messengerService, MessengerDbContext context)
    {
        _messengerService = messengerService;
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            _connectionToUserId[Context.ConnectionId] = userId.Value;
            _messengerService.AddOnlineUser(userId.Value);
            
            var user = await _messengerService.GetUserByIdAsync(userId.Value);
            if (user != null)
            {
                await Clients.All.SendAsync("UserJoined", user.Username);
                await Clients.All.SendAsync("UserListUpdate", await GetUserListAsync());
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionToUserId.TryGetValue(Context.ConnectionId, out var userId))
        {
            _messengerService.RemoveOnlineUser(userId);
            
            var user = await _messengerService.GetUserByIdAsync(userId);
            if (user != null)
            {
                await Clients.All.SendAsync("UserLeft", user.Username);
                await Clients.All.SendAsync("UserListUpdate", await GetUserListAsync());
            }
            
            _connectionToUserId.Remove(Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task LoadChatHistory(string toUsername)
    {
        var fromUserId = GetUserId();
        if (!fromUserId.HasValue) return;

        var toUser = await _messengerService.GetUserByUsernameAsync(toUsername);
        if (toUser == null) return;

        var history = await _messengerService.GetChatHistoryAsync(fromUserId.Value, toUser.Id, limit: 50);
        
        var historyData = history.Select(m => new
        {
            fromUsername = m.Sender.Username,
            toUsername = m.Receiver.Username,
            message = m.Text,
            createdAt = m.CreatedAt
        }).ToList();

        await Clients.Caller.SendAsync("ChatHistoryLoaded", toUsername, historyData);
    }

    public async Task SendMessage(string toUsername, string message)
    {
        var fromUserId = GetUserId();
        if (!fromUserId.HasValue || string.IsNullOrWhiteSpace(message)) 
            return;

        var toUser = await _messengerService.GetUserByUsernameAsync(toUsername);
        if (toUser == null) return;

        var fromUser = await _messengerService.GetUserByIdAsync(fromUserId.Value);
        if (fromUser == null) return;

        // Save to database
        var savedMessage = await _messengerService.SaveMessageAsync(fromUserId.Value, toUser.Id, message);

        // Send to all clients (frontend filters)
        await Clients.All.SendAsync("ReceiveMessage", fromUser.Username, toUsername, message, savedMessage.CreatedAt);
    }

    // WebRTC signaling
    public async Task SendOffer(string toUsername, string offer)
    {
        var fromUserId = GetUserId();
        if (!fromUserId.HasValue) return;

        var fromUser = await _messengerService.GetUserByIdAsync(fromUserId.Value);
        if (fromUser == null) return;

        await Clients.All.SendAsync("ReceiveOffer", fromUser.Username, toUsername, offer);
    }

    public async Task SendAnswer(string toUsername, string answer)
    {
        var fromUserId = GetUserId();
        if (!fromUserId.HasValue) return;

        var fromUser = await _messengerService.GetUserByIdAsync(fromUserId.Value);
        if (fromUser == null) return;

        await Clients.All.SendAsync("ReceiveAnswer", fromUser.Username, toUsername, answer);
    }

    public async Task SendIceCandidate(string toUsername, string candidate)
    {
        var fromUserId = GetUserId();
        if (!fromUserId.HasValue) return;

        var fromUser = await _messengerService.GetUserByIdAsync(fromUserId.Value);
        if (fromUser == null) return;

        await Clients.All.SendAsync("ReceiveIceCandidate", fromUser.Username, toUsername, candidate);
    }

    private int? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    private async Task<List<string>> GetUserListAsync()
    {
        var onlineUsers = await _messengerService.GetOnlineUsersAsync();
        return onlineUsers.Select(u => u.Username).ToList();
    }
}
