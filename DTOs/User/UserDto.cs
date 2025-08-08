namespace Api.DTOs.User;

public class UserDto
{
    public required int Id { get; set; }
    public string? Identity { get; set; }
    public string? Username { get; set; } = string.Empty;
    public string? Permission { get; set; } = string.Empty;
}