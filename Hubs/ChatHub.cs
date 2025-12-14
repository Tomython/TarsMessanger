using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize]  // Только авторизованные
public class ChatHub : Hub
{

    private static Dictionary<string, string> _users = new();
    private static Dictionary<string, HashSet<string>> _connections = new();

    public override async Task OnConnectedAsync()
    {
        _connections[Context.ConnectionId] = new HashSet<string>();
        await base.OnConnectedAsync();
    }

    public async Task Join(string username)
    {
        _users[Context.ConnectionId] = username;
        await Clients.All.SendAsync("UserJoined", username);
    }

    public async Task SendMessage(string toUsername, string message)
    {
        if (_users.TryGetValue(Context.ConnectionId, out var fromUsername))
        {
            await Clients.All.SendAsync("ReceiveMessage", fromUsername, toUsername, message);
        }
    }


}
