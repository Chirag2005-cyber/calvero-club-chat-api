using Api.Entities;

namespace Api.Services.UserService;

public interface IUserService
{
    Task<User?> GetUserByTokenAsync(string token);
}