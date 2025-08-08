using Api.Common;

namespace Api.Entities;

public class User
{
    public int Id { get; set; }
    public required string Identity { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Permission { get; set; } = AppConstants.Roles.User;
}
