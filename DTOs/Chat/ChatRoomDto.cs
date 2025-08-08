namespace Api.DTOs.Chat;

public class ChatRoomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ParticipantCount { get; set; }
    public bool IsJoined { get; set; }
}