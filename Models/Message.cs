namespace TarsMessanger.Models;

public class Message
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    
    // Navigation properties
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}
