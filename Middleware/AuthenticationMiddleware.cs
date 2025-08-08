using Api.Entities;
using Api.Services.UserService;

namespace Api.Middleware;

public class Authentication
{
    private readonly RequestDelegate _next;

    public Authentication(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            var userService = context.RequestServices.GetRequiredService<IUserService>();
            User? user = await userService.GetUserByTokenAsync(token);

            if (user != null)
            {
                context.Items["User"] = user;
            }
        }

        await _next(context);
    }
}