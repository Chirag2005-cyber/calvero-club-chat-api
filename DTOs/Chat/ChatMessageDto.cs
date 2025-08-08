namespace Api.DTOs.Chat;

public class ChatMessageDto
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public MessageAuthorDto Author { get; set; } = new();
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}