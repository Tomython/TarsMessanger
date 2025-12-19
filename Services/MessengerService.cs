using Microsoft.EntityFrameworkCore;
using TarsMessanger.Data;
using TarsMessanger.Models;

namespace TarsMessanger.Services;

public class MessengerService : IMessengerService
{
    private readonly MessengerDbContext _context;
    private static readonly HashSet<int> _onlineUserIds = new();

    public MessengerService(MessengerDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<List<User>> GetOnlineUsersAsync()
    {
        var onlineIds = _onlineUserIds.ToList();
        return await _context.Users
            .Where(u => onlineIds.Contains(u.Id))
            .ToListAsync();
    }

    public void AddOnlineUser(int userId)
    {
        _onlineUserIds.Add(userId);
    }

    public void RemoveOnlineUser(int userId)
    {
        _onlineUserIds.Remove(userId);
    }

    public async Task<Message> SaveMessageAsync(int senderId, int receiverId, string text)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Text = text,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<List<Message>> GetChatHistoryAsync(int userId1, int userId2, int limit = 50)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                       (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkMessagesAsReadAsync(int senderId, int receiverId)
    {
        var unreadMessages = await _context.Messages
            .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }
}
