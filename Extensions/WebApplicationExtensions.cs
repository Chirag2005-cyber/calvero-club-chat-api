using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Hubs;
using Api.Middleware;

namespace Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Calvero Chat API v1");
            options.DocumentTitle = "Calvero Chat API";
            options.RoutePrefix = string.Empty;
        });

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors("AllowClients");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSignalRLogging();
        app.UseMiddleware<Authentication>();

        return app;
    }

    public static WebApplication ConfigureEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapHub<ChatHub>("/chathub");
        
        return app;
    }

    private static WebApplication UseSignalRLogging(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/chathub"))
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogDebug($"SignalR connection attempt from {context.Connection.RemoteIpAddress} to {context.Request.Path}");

                var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

                if (string.IsNullOrEmpty(accessToken) && string.IsNullOrEmpty(authHeader))
                {
                    logger.LogWarning("SignalR connection attempt without any token");
                }
            }

            await next();
        });

        return app;
    }
}
