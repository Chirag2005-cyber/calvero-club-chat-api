using Microsoft.AspNetCore.Mvc;
using Api.Services.ChatService;
using Api.DTOs.Chat;
using Api.Attributes;

namespace Api.Controllers;

[Route("chat")]
[RequiredUser]
public class ChatController : BaseController
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("rooms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomDto createDto)
    {
        try
        {
            var chatRoom = await _chatService.CreateChatRoomAsync(createDto, CurrentUserId!, CurrentUsername!);
            return Success(chatRoom, "Chat room created successfully.");
        }
        catch (Exception ex)
        {
            return Failure($"Failed to create chat room: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("rooms/join")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> JoinChatRoom([FromBody] JoinChatRoomDto joinDto)
    {
        var result = await _chatService.JoinChatRoomAsync(joinDto, CurrentUserId!, CurrentUsername!);
        
        if (!result)
            return BadRequest("Invalid chat room or password.");

        return Success(message: "Successfully joined chat room.");
    }

    [HttpGet("rooms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserChatRooms()
    {
        var chatRooms = await _chatService.GetUserChatRoomsAsync(CurrentUserId!);
        return Success(chatRooms);
    }

    [HttpGet("rooms/public")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPublicChatRooms()
    {
        var chatRooms = await _chatService.GetPublicChatRoomsAsync(CurrentUserId!);
        return Success(chatRooms);
    }

    [HttpGet("rooms/{chatRoomId}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetChatMessages(int chatRoomId)
    {
        var messages = await _chatService.GetChatMessagesAsync(chatRoomId, CurrentUserId!);
        return Success(messages);
    }

    [HttpPost("rooms/{chatRoomId}/leave")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LeaveChatRoom(int chatRoomId)
    {
        var result = await _chatService.LeaveChatRoomAsync(chatRoomId, CurrentUserId!);
        
        if (!result)
            return BadRequest("You are not a member of this chat room.");

        return Success(message: "Successfully left chat room.");
    }

    [HttpPost("messages")]
    [RateLimit(MaxRequests = 5, TimeWindowSeconds = 30)] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto messageDto)
    {
        var message = await _chatService.SendMessageAsync(messageDto, CurrentUserId!, CurrentUsername!);
        
        if (message == null)
            return BadRequest("Unable to send message. Check your access to the chat room.");

        return Success(message, "Message sent successfully.");
    }
}
