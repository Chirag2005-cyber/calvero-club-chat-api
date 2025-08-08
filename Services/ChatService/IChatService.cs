using Api.DTOs.Chat;

namespace Api.Services.ChatService;

public interface IChatService
{
    Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomDto createDto, string userId, string author);
    Task<bool> JoinChatRoomAsync(JoinChatRoomDto joinDto, string userId, string author);
    Task<List<ChatRoomDto>> GetUserChatRoomsAsync(string userId);
    Task<List<ChatMessageDto>> GetChatMessagesAsync(int chatRoomId, string userId);
    Task<ChatMessageDto?> SendMessageAsync(SendMessageDto messageDto, string userId, string author);
    Task<bool> LeaveChatRoomAsync(int chatRoomId, string userId);
    Task<List<ChatRoomDto>> GetPublicChatRoomsAsync(string userId);
    Task<bool> ValidateChatRoomAccessAsync(int chatRoomId, string userId);
}
