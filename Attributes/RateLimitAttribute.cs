using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;
using Api.Common;

namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RateLimitAttribute : ActionFilterAttribute
{
    private static readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
    
    public int MaxRequests { get; set; } = 10;
    
    public int TimeWindowSeconds { get; set; } = 60;
    
    public string? ErrorMessage { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var httpContext = context.HttpContext;
        
        var userId = httpContext.User.FindFirst("UserId")?.Value;
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var key = userId ?? ipAddress ?? "anonymous";
        
        var now = DateTime.UtcNow;
        var timeWindow = TimeSpan.FromSeconds(TimeWindowSeconds);
        
        _requests.AddOrUpdate(key, 
            new List<DateTime> { now },
            (k, requests) =>
            {
                requests.RemoveAll(r => now - r > timeWindow);
                requests.Add(now);
                return requests;
            });

        var currentRequests = _requests[key];
        
        if (currentRequests.Count > MaxRequests)
        {
            var result = new ApiResult<object>
            {
                Success = false,
                Message = ErrorMessage ?? $"Rate limit exceeded. Maximum {MaxRequests} requests per {TimeWindowSeconds} seconds.",
                Errors = new List<string> { $"Too many requests. Please wait {TimeWindowSeconds} seconds before trying again." }
            };

            context.Result = new ObjectResult(result) { StatusCode = StatusCodes.Status429TooManyRequests };
            return;
        }

        base.OnActionExecuting(context);
    }
}
