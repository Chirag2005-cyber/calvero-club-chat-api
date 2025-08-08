namespace Api.Entities;

public class ChatRoom
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EncryptedPassword { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    public User CreatedByAuthor { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
}
