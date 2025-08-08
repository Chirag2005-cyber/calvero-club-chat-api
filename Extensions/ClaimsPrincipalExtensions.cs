using System.Security.Claims;
using Api.Common;
using Api.Services.EncryptionService;

namespace Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal, IServiceProvider? serviceProvider = null)
    {
        var encryptedUserId = principal?.FindFirst(AppConstants.ClaimTypes.UserId)?.Value;
        if (string.IsNullOrEmpty(encryptedUserId) || serviceProvider == null)
            return encryptedUserId;

        try
        {
            var encryptionService = serviceProvider.GetService<IEncryptionService>();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var jwtKey = configuration?.GetSection("Jwt")["Key"] ?? string.Empty;
            return encryptionService?.DecryptClaim(encryptedUserId, jwtKey);
        }
        catch
        {
            return null;
        }
    }

    public static string? GetUsername(this ClaimsPrincipal principal, IServiceProvider? serviceProvider = null)
    {
        var encryptedUsername = principal?.FindFirst(AppConstants.ClaimTypes.Username)?.Value;
        if (string.IsNullOrEmpty(encryptedUsername) || serviceProvider == null)
            return encryptedUsername;

        try
        {
            var encryptionService = serviceProvider.GetService<IEncryptionService>();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var jwtKey = configuration?.GetSection("Jwt")["Key"] ?? string.Empty;
            return encryptionService?.DecryptIdentity(encryptedUsername, jwtKey);
        }
        catch
        {
            return null;
        }
    }

    public static string? GetIdentity(this ClaimsPrincipal principal, IServiceProvider? serviceProvider = null)
    {
        var encryptedIdentity = principal?.FindFirst(AppConstants.ClaimTypes.Identity)?.Value;
        if (string.IsNullOrEmpty(encryptedIdentity) || serviceProvider == null)
            return encryptedIdentity;

        try
        {
            var encryptionService = serviceProvider.GetService<IEncryptionService>();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var jwtKey = configuration?.GetSection("Jwt")["Key"] ?? string.Empty;
            return encryptionService?.DecryptIdentity(encryptedIdentity, jwtKey);
        }
        catch
        {
            return null;
        }
    }

    public static bool IsAuthenticated(this ClaimsPrincipal principal)
    {
        return !string.IsNullOrEmpty(principal?.FindFirst(AppConstants.ClaimTypes.UserId)?.Value);
    }
}
