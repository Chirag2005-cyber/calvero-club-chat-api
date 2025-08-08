using Api.DTOs;
using Api.Services.JwtService;
using Api.Services.EncryptionService;
using Api.Entities;
using Api.Data;
using Api.Common;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IJwtService jwtService, IEncryptionService encryptionService, IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _encryptionService = encryptionService;
        _configuration = configuration;
    }

    public AuthenticateDto? Authenticate(AuthenticateDto request)
    {
        var user = _context.Users.SingleOrDefault(u => u.Identity == request.Identity);
        if (user == null)
        {
            return null;
        }

        var jwtKey = _configuration.GetSection("Jwt")["Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
        var encryptedIdentity = _encryptionService.EncryptIdentity(user.Identity, jwtKey);
        var encryptedUserId = _encryptionService.EncryptClaim(user.Id.ToString(), jwtKey);

        var claims = new[]
        {
            new Claim(AppConstants.ClaimTypes.Username, encryptedIdentity),
            new Claim(AppConstants.ClaimTypes.UserId, encryptedUserId)
        };

        var token = _jwtService.GenerateToken(claims);
        return new AuthenticateDto
        {
            Token = token
        };
    }

    public string CreateAccount(CreateUserDto request)
    {
        int attempts = 0;
        string identity;

        do
        {
            identity = Guid.NewGuid().ToString();
            attempts++;

            if (attempts >= AppConstants.Validation.MaxRetryAttempts)
            {
                throw new InvalidOperationException($"Unable to generate a unique identity after {AppConstants.Validation.MaxRetryAttempts} attempts.");
            }
        } while (_context.Users.Any(u => u.Identity == identity));

        var user = new User
        {
            Identity = identity,
            Username = request.Username,
            Permission = AppConstants.Roles.User
        };

        try
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new InvalidOperationException("Identity collision detected. Please try again.", ex);
        }

        return identity;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true ||
               ex.InnerException?.Message.Contains("duplicate key") == true;
    }
}