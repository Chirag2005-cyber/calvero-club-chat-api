using Microsoft.EntityFrameworkCore;
using Api.Entities;
using Api.Common;

namespace Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatParticipant> ChatParticipants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Identity).IsUnique();
            entity.Property(e => e.Identity).IsRequired();
        });

        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(AppConstants.Validation.MaxChatRoomNameLength);
            entity.Property(e => e.EncryptedPassword).IsRequired();
            
            entity.HasOne(e => e.CreatedByAuthor)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EncryptedContent).IsRequired();
            
            entity.HasOne(e => e.ChatRoom)
                .WithMany(cr => cr.Messages)
                .HasForeignKey(e => e.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Author)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChatParticipant>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.ChatRoom)
                .WithMany(cr => cr.Participants)
                .HasForeignKey(e => e.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Author)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex("ChatRoomId", "AuthorId").IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}