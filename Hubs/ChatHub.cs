using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize]  // Только авторизованные
public class ChatHub : Hub
{
    private static Dictionary<string, string> OnlineUsers = new();

    public async Task Join(string username)
    {
        OnlineUsers[username] = Context.ConnectionId;
        await Clients.All.SendAsync("UserJoined", username);
    }

    public async Task SendMessage(string toUsername, string message)
    {
        if (OnlineUsers.TryGetValue(toUsername, out var connId))
            await Clients.Client(connId).SendAsync("ReceiveMessage", Context.UserIdentifier, message);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = OnlineUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
        OnlineUsers.Remove(username);
        await base.OnDisconnectedAsync(exception);
    }
}
