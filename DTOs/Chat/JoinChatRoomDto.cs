namespace Api.DTOs.Chat;

public class JoinChatRoomDto
{
    public int ChatRoomId { get; set; }
    public string Password { get; set; } = string.Empty;
}
