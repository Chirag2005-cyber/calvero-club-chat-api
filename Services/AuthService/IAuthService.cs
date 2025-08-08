using Api.DTOs;

namespace Api.Services.AuthService;

public interface IAuthService
{
    AuthenticateDto? Authenticate(AuthenticateDto request);
    string CreateAccount(CreateUserDto request);

}