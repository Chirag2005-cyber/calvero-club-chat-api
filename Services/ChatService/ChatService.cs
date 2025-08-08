using Api.Data;
using Api.DTOs.Chat;
using Api.Entities;
using Api.Services.EncryptionService;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ChatService;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;

    public ChatService(AppDbContext context, IEncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
    }

    public async Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomDto createDto, string userId, string username)
    {
        if (!int.TryParse(userId, out int authorId))
            throw new ArgumentException("Invalid user ID");

        var user = await _context.Users.FindAsync(authorId);
        if (user == null)
            throw new ArgumentException("User not found");

        var chatRoom = new ChatRoom
        {
            Name = createDto.Name,
            EncryptedPassword = _encryptionService.HashPassword(createDto.Password),
            CreatedByAuthor = user,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatRooms.Add(chatRoom);
        await _context.SaveChangesAsync();

        var participant = new ChatParticipant
        {
            ChatRoomId = chatRoom.Id,
            Author = user,
            JoinedAt = DateTime.UtcNow,
            IsOnline = true
        };

        _context.ChatParticipants.Add(participant);
        await _context.SaveChangesAsync();

        return new ChatRoomDto
        {
            Id = chatRoom.Id,
            Name = chatRoom.Name,
            CreatedByUsername = user.Username,
            CreatedAt = chatRoom.CreatedAt,
            ParticipantCount = 1,
            IsJoined = true
        };
    }

    public async Task<bool> JoinChatRoomAsync(JoinChatRoomDto joinDto, string userId, string username)
    {
        if (!int.TryParse(userId, out int authorId))
            return false;

        var user = await _context.Users.FindAsync(authorId);
        if (user == null)
            return false;

        var chatRoom = await _context.ChatRooms
            .FirstOrDefaultAsync(cr => cr.Id == joinDto.ChatRoomId && cr.IsActive);

        if (chatRoom == null)
            return false;

        if (!_encryptionService.VerifyPassword(joinDto.Password, chatRoom.EncryptedPassword))
            return false;

        var existingParticipant = await _context.ChatParticipants
            .Include(cp => cp.Author)
            .FirstOrDefaultAsync(cp => cp.ChatRoomId == joinDto.ChatRoomId && cp.Author.Id == authorId);

        if (existingParticipant != null)
        {
            existingParticipant.IsOnline = true;
            await _context.SaveChangesAsync();
            return true;
        }

        var participant = new ChatParticipant
        {
            ChatRoomId = joinDto.ChatRoomId,
            Author = user,
            JoinedAt = DateTime.UtcNow,
            IsOnline = true
        };

        _context.ChatParticipants.Add(participant);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ChatRoomDto>> GetUserChatRoomsAsync(string userId)
    {
        if (!int.TryParse(userId, out int authorId))
            return new List<ChatRoomDto>();

        var userChatRooms = await _context.ChatParticipants
            .Where(cp => cp.Author.Id == authorId)
            .Include(cp => cp.ChatRoom)
            .ThenInclude(cr => cr.CreatedByAuthor)
            .Include(cp => cp.ChatRoom)
            .ThenInclude(cr => cr.Participants)
            .Where(cp => cp.ChatRoom.IsActive)
            .Select(cp => new ChatRoomDto
            {
                Id = cp.ChatRoom.Id,
                Name = cp.ChatRoom.Name,
                CreatedByUsername = cp.ChatRoom.CreatedByAuthor.Username,
                CreatedAt = cp.ChatRoom.CreatedAt,
                ParticipantCount = cp.ChatRoom.Participants.Count,
                IsJoined = true
            })
            .ToListAsync();

        return userChatRooms;
    }

    public async Task<List<ChatMessageDto>> GetChatMessagesAsync(int chatRoomId, string userId)
    {
        if (!int.TryParse(userId, out int authorId))
            return new List<ChatMessageDto>();

        var hasAccess = await _context.ChatParticipants
            .AnyAsync(cp => cp.ChatRoomId == chatRoomId && cp.Author.Id == authorId);

        if (!hasAccess)
            return new List<ChatMessageDto>();

        var chatRoom = await _context.ChatRooms.FindAsync(chatRoomId);
        if (chatRoom == null)
            return new List<ChatMessageDto>();

        var messages = await _context.ChatMessages
            .Where(cm => cm.ChatRoomId == chatRoomId)
            .Include(cm => cm.Author)
            .OrderBy(cm => cm.SentAt)
            .ToListAsync();

        var decryptedMessages = new List<ChatMessageDto>();

        foreach (var message in messages)
        {
            var decryptedContent = _encryptionService.Decrypt(message.EncryptedContent, chatRoom.EncryptedPassword);

            decryptedMessages.Add(new ChatMessageDto
            {
                Id = message.Id,
                ChatRoomId = message.ChatRoomId,
                Author = new MessageAuthorDto
                {
                    Id = message.Author.Id.ToString(),
                    Username = message.Author.Username
                },
                Content = decryptedContent,
                SentAt = message.SentAt
            });
        }

        return decryptedMessages;
    }

    public async Task<ChatMessageDto?> SendMessageAsync(SendMessageDto messageDto, string userId, string username)
    {
        if (!int.TryParse(userId, out int authorId))
            return null;

        var user = await _context.Users.FindAsync(authorId);
        if (user == null)
            return null;

        var participant = await _context.ChatParticipants
            .Include(cp => cp.Author)
            .FirstOrDefaultAsync(cp => cp.ChatRoomId == messageDto.ChatRoomId && cp.Author.Id == authorId);

        if (participant == null)
            return null;

        var chatRoom = await _context.ChatRooms.FindAsync(messageDto.ChatRoomId);
        if (chatRoom == null)
            return null;

        var encryptedContent = _encryptionService.Encrypt(messageDto.Content, chatRoom.EncryptedPassword);

        var message = new ChatMessage
        {
            ChatRoomId = messageDto.ChatRoomId,
            Author = user,
            EncryptedContent = encryptedContent,
            SentAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        return new ChatMessageDto
        {
            Id = message.Id,
            ChatRoomId = message.ChatRoomId,
            Author = new MessageAuthorDto
            {
                Id = user.Id.ToString(),
                Username = user.Username
            },
            Content = messageDto.Content,
            SentAt = message.SentAt
        };
    }

    public async Task<bool> LeaveChatRoomAsync(int chatRoomId, string userId)
    {
        if (!int.TryParse(userId, out int authorId))
            return false;

        var participant = await _context.ChatParticipants
            .Include(cp => cp.Author)
            .FirstOrDefaultAsync(cp => cp.ChatRoomId == chatRoomId && cp.Author.Id == authorId);

        if (participant == null)
            return false;

        participant.IsOnline = false;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ChatRoomDto>> GetPublicChatRoomsAsync(string userId)
    {
        if (!int.TryParse(userId, out int authorId))
            return new List<ChatRoomDto>();

        var allChatRooms = await _context.ChatRooms
            .Where(cr => cr.IsActive)
            .Include(cr => cr.CreatedByAuthor)
            .Include(cr => cr.Participants)
            .ToListAsync();

        var userChatRoomIds = await _context.ChatParticipants
            .Where(cp => cp.Author.Id == authorId)
            .Select(cp => cp.ChatRoomId)
            .ToListAsync();

        return allChatRooms.Select(cr => new ChatRoomDto
        {
            Id = cr.Id,
            Name = cr.Name,
            CreatedByUsername = cr.CreatedByAuthor.Username,
            CreatedAt = cr.CreatedAt,
            ParticipantCount = cr.Participants.Count,
            IsJoined = userChatRoomIds.Contains(cr.Id)
        }).ToList();
    }

    public async Task<bool> ValidateChatRoomAccessAsync(int chatRoomId, string userId)
    {
        if (!int.TryParse(userId, out int authorId))
            return false;

        return await _context.ChatParticipants
            .AnyAsync(cp => cp.ChatRoomId == chatRoomId && cp.Author.Id == authorId);
    }
}
