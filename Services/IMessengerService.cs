using TarsMessanger.Models;

namespace TarsMessanger.Services;

public interface IMessengerService
{
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<List<User>> GetOnlineUsersAsync();
    Task<Message> SaveMessageAsync(int senderId, int receiverId, string text);
    Task<List<Message>> GetChatHistoryAsync(int userId1, int userId2, int limit = 50);
    Task MarkMessagesAsReadAsync(int senderId, int receiverId);
    void AddOnlineUser(int userId);
    void RemoveOnlineUser(int userId);
}
