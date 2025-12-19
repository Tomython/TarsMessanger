using Microsoft.EntityFrameworkCore;
using TarsMessanger.Models;

namespace TarsMessanger.Data;

public class MessengerDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }

    public MessengerDbContext(DbContextOptions<MessengerDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Relationships
            entity.HasOne(e => e.Sender)
                  .WithMany(u => u.SentMessages)
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Receiver)
                  .WithMany(u => u.ReceivedMessages)
                  .HasForeignKey(e => e.ReceiverId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes for fast 1:1 chat queries
            entity.HasIndex(e => new { e.SenderId, e.ReceiverId, e.CreatedAt });
            entity.HasIndex(e => new { e.ReceiverId, e.SenderId, e.CreatedAt });
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
