using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace Api.Extensions;

public static class JwtEventHandlers
{
    public static Task OnMessageReceived(MessageReceivedContext context)
    {
        var accessToken = context.Request.Query["access_token"].FirstOrDefault();
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        var path = context.HttpContext.Request.Path;

        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
        {
            var cleanToken = accessToken.StartsWith("Bearer ") 
                ? accessToken.Substring("Bearer ".Length).Trim() 
                : accessToken;
            context.Token = cleanToken;
        }
        else if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (token.StartsWith("Bearer "))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }
            context.Token = token;
        }

        return Task.CompletedTask;
    }

    public static Task OnAuthenticationFailed(AuthenticationFailedContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError($"Authentication failed: {context.Exception.Message} | Path: {context.Request.Path}");
        return Task.CompletedTask;
    }

    public static Task OnTokenValidated(TokenValidatedContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        logger.LogDebug($"Token validated successfully for user: {userId}");
        return Task.CompletedTask;
    }
}
