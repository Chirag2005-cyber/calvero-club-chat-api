using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Api.Services.ChatService;
using Api.Services.EncryptionService;
using Api.DTOs.Chat;
using Api.Common;

namespace Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;
    private readonly IEncryptionService _encryptionService;
    private readonly IConfiguration _configuration;

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger, IEncryptionService encryptionService, IConfiguration configuration)
    {
        _chatService = chatService;
        _logger = logger;
        _encryptionService = encryptionService;
        _configuration = configuration;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var (userId, username) = GetDecryptedUserInfo();

            _logger.LogInformation($"User {username} ({userId}) connected to ChatHub with ConnectionId: {Context.ConnectionId}");

            if (!string.IsNullOrEmpty(userId))
            {
                var userChatRooms = await _chatService.GetUserChatRoomsAsync(userId);
                
                foreach (var chatRoom in userChatRooms)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"ChatRoom_{chatRoom.Id}");
                }

                foreach (var chatRoom in userChatRooms)
                {
                    await Clients.Group($"ChatRoom_{chatRoom.Id}")
                        .SendAsync("UserJoined", new { username, chatRoomId = chatRoom.Id });
                }
            }
            else
            {
                _logger.LogWarning("User connected without valid UserId. ConnectionId: {ConnectionId}", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OnConnectedAsync for ConnectionId: {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var (userId, username) = GetDecryptedUserInfo();

            _logger.LogInformation("User {Username} ({UserId}) disconnected from ChatHub. ConnectionId: {ConnectionId}", 
                username, userId, Context.ConnectionId);

            if (exception != null)
            {
                _logger.LogError(exception, "User disconnected with exception. ConnectionId: {ConnectionId}", Context.ConnectionId);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                var userChatRooms = await _chatService.GetUserChatRoomsAsync(userId);
                
                foreach (var chatRoom in userChatRooms)
                {
                    await _chatService.LeaveChatRoomAsync(chatRoom.Id, userId);
                    await Clients.Group($"ChatRoom_{chatRoom.Id}")
                        .SendAsync(AppConstants.SignalR.UserLeftEvent, new { username, chatRoomId = chatRoom.Id });
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OnDisconnectedAsync for ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
    }

    private (string? userId, string? username) GetDecryptedUserInfo()
    {
        try
        {
            var encryptedUserId = Context.User?.FindFirst(AppConstants.ClaimTypes.UserId)?.Value;
            var encryptedUsername = Context.User?.FindFirst(AppConstants.ClaimTypes.Username)?.Value;

            var jwtKey = _configuration.GetSection("Jwt")["Key"] ?? throw new InvalidOperationException("JWT Key is not configured");

            var userId = !string.IsNullOrEmpty(encryptedUserId) 
                ? _encryptionService.DecryptClaim(encryptedUserId, jwtKey) 
                : null;
            
            var username = !string.IsNullOrEmpty(encryptedUsername) 
                ? _encryptionService.DecryptIdentity(encryptedUsername, jwtKey) 
                : null;

            return (userId, username);
        }
        catch
        {
            return (null, null);
        }
    }

    public async Task JoinChatRoom(int chatRoomId)
    {
        try
        {
            var (userId, username) = GetDecryptedUserInfo();

            _logger.LogInformation($"JoinChatRoom called by {username} ({userId}) for ChatRoom {chatRoomId}");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("JoinChatRoom called without valid user credentials");
                return;
            }

            var hasAccess = await _chatService.ValidateChatRoomAccessAsync(chatRoomId, userId);
            
            if (hasAccess)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"ChatRoom_{chatRoomId}");
                
                await Clients.Group($"ChatRoom_{chatRoomId}")
                    .SendAsync(AppConstants.SignalR.UserJoinedEvent, new { username, chatRoomId });

                _logger.LogInformation($"User {username} successfully joined ChatRoom {chatRoomId}");
            }
            else
            {
                _logger.LogWarning($"User {userId} attempted to join unauthorized ChatRoom {chatRoomId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error joining ChatRoom {chatRoomId}");
            throw;
        }
    }

    public async Task LeaveChatRoom(int chatRoomId)
    {
        try
        {
            var (userId, username) = GetDecryptedUserInfo();

            _logger.LogInformation($"LeaveChatRoom called by {username} ({userId}) for ChatRoom {chatRoomId}");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            {
                _logger.LogWarning($"LeaveChatRoom called without valid user credentials");
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ChatRoom_{chatRoomId}");
            await _chatService.LeaveChatRoomAsync(chatRoomId, userId);
            
            await Clients.Group($"ChatRoom_{chatRoomId}")
                .SendAsync(AppConstants.SignalR.UserLeftEvent, new { username, chatRoomId });

            _logger.LogInformation($"User {username} successfully left ChatRoom {chatRoomId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error leaving ChatRoom {chatRoomId}");
            throw;
        }
    }

    public async Task SendMessage(int chatRoomId, string content)
    {
        try
        {
            var (userId, username) = GetDecryptedUserInfo();

            _logger.LogInformation($"SendMessage called by {username} ({userId}) for ChatRoom {chatRoomId}");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("SendMessage called without valid user credentials");
                return;
            }

            var hasAccess = await _chatService.ValidateChatRoomAccessAsync(chatRoomId, userId);
            
            if (!hasAccess)
            {
                _logger.LogWarning($"User {userId} attempted to send message to unauthorized ChatRoom {chatRoomId}");
                return;
            }

            var messageDto = new SendMessageDto
            {
                ChatRoomId = chatRoomId,
                Content = content
            };

            var message = await _chatService.SendMessageAsync(messageDto, userId, username);
            
            if (message != null)
            {
                await Clients.Group($"ChatRoom_{chatRoomId}")
                    .SendAsync("ReceiveMessage", message);
                
                _logger.LogInformation("Message sent successfully to ChatRoom {ChatRoomId}", chatRoomId);
            }
            else
            {
                _logger.LogError("Failed to send message to ChatRoom {ChatRoomId}", chatRoomId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending message to ChatRoom {chatRoomId}");
            throw;
        }
    }

    public async Task SendEncryptedMessage(int chatRoomId, string encryptedContent)
    {
        try
        {
            var (userId, username) = GetDecryptedUserInfo();

            _logger.LogInformation($"SendEncryptedMessage called by {username} ({userId}) for ChatRoom {chatRoomId}");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("SendEncryptedMessage called without valid user credentials");
                return;
            }

            var hasAccess = await _chatService.ValidateChatRoomAccessAsync(chatRoomId, userId);
            
            if (!hasAccess)
            {
                _logger.LogWarning($"User {userId} attempted to send encrypted message to unauthorized ChatRoom {chatRoomId}");
                return;
            }

            var encryptedMessage = new
            {
                chatRoomId,
                authorId = userId,
                authorUsername = username,
                encryptedContent,
                sentAt = DateTime.UtcNow
            };

            await Clients.Group($"ChatRoom_{chatRoomId}")
                .SendAsync("ReceiveEncryptedMessage", encryptedMessage);

            _logger.LogInformation($"Encrypted message sent successfully to ChatRoom {chatRoomId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending encrypted message to ChatRoom {chatRoomId}");
            throw;
        }
    }
}
