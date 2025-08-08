namespace Api.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public string EncryptedContent { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    public ChatRoom ChatRoom { get; set; } = null!;
    public User Author { get; set; } = null!;
}
