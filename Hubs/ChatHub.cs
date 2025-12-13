using Microsoft.AspNetCore.SignalR;
public class ChatHub : Hub
{
    private static Dictionary<string, string> Users = new(); // username -> connectionId
    public async Task Join(string username)
    {
        Users[username] = Context.ConnectionId;
        await Clients.All.SendAsync("UserJoined", username);
    }
    public async Task SendMessage(string toUsername, string message)
    {
        if (Users.TryGetValue(toUsername, out var connId))
            await Clients.Client(connId).SendAsync("ReceiveMessage", Context.ConnectionId, message);
    }
    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var username = Users.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
        Users.Remove(username);
        await base.OnDisconnectedAsync(ex);
    }
}
