namespace Api.DTOs.Chat;

public class SendMessageDto
{
    public int ChatRoomId { get; set; }
    public string Content { get; set; } = string.Empty;
}
