namespace Api.Entities;

public class ChatParticipant
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsOnline { get; set; } = false;
    
    public ChatRoom ChatRoom { get; set; } = null!;
    public User Author { get; set; } = null!;
}
