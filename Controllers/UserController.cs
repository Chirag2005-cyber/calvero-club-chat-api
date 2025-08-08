using Microsoft.AspNetCore.Mvc;
using Api.DTOs.User;
using Api.Entities;
using Api.Attributes;

namespace Api.Controllers;

[Route("users")]
public class UserController : BaseController
{
    [HttpGet("me")]
    [RequiredUser]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Me()
    {
        User? user = HttpContext.Items["User"] as User;

        if (user == null)
            return NotFound("User not found");

        var response = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Permission = user.Permission
        };

        return Success(response);
    }
}