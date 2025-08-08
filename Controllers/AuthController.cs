using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Api.Services.AuthService;

namespace Api.Controllers;

[Route("auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] AuthenticateDto request)
    {
        var result = _authService.Authenticate(request);

        if (result == null)
            return Unauthorized("Invalid identity.");

        return Success(result, "Login successful.");
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Register([FromBody] CreateUserDto request)
    {
        try
        {
            var identity = _authService.CreateAccount(request);
            return Success(new { identity }, "Registration successful.");
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ex.Message, StatusCodes.Status409Conflict);
        }
    }
}