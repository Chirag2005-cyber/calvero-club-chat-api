using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Services.JwtService;

public interface IJwtService
{
    string GenerateToken(IEnumerable<Claim> claims);
    string GenerateToken(string username);
    JwtSecurityToken DecodeToken(string token);
    ClaimsPrincipal? ValidateToken(string token);
    DateTime GetExpirationTime();
}