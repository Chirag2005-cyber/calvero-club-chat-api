using Api.Data;
using Api.Entities;
using Api.Services.JwtService;
using Api.Services.EncryptionService;
using Api.Common;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UserService;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IEncryptionService _encryptionService;
    private readonly IConfiguration _configuration;

    public UserService(AppDbContext context, IJwtService jwtService, IEncryptionService encryptionService, IConfiguration configuration)
    {
        _jwtService = jwtService;
        _context = context;
        _encryptionService = encryptionService;
        _configuration = configuration;
    }

    public async Task<User?> GetUserByTokenAsync(string token)
    {
        var principal = _jwtService.ValidateToken(token);
        if (principal == null)
            return null;

        var claims = principal.Claims;
        var encryptedIdentityClaim = claims?.FirstOrDefault(c => c.Type == AppConstants.ClaimTypes.Username)
            ?? claims?.FirstOrDefault(c => c.Type == AppConstants.ClaimTypes.Identity);

        if (string.IsNullOrEmpty(encryptedIdentityClaim?.Value))
            return null;

        try
        {
            var jwtKey = _configuration.GetSection("Jwt")["Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var identity = _encryptionService.DecryptIdentity(encryptedIdentityClaim.Value, jwtKey);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Identity == identity);
            if (user == null)
            {
                return null;
            }

            return new User
            {
                Id = user.Id,
                Permission = user.Permission,
                Identity = user.Identity,
                Username = user.Username
            };
        }
        catch
        {
            return null;
        }
    }
}