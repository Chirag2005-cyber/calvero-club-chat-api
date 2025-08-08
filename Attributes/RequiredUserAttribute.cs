using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Api.Common;

namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequiredUserAttribute : ActionFilterAttribute
{
    public int MinimumPermission { get; set; } = 0;

    public string? ErrorMessage { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var httpContext = context.HttpContext;
        
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            var result = new ApiResult<object>
            {
                Success = false,
                Message = ErrorMessage ?? "Authentication required. Please provide a valid JWT token.",
                Errors = new List<string> { "User authentication is required to access this endpoint." }
            };

            context.Result = new UnauthorizedObjectResult(result);
            return;
        }

        var userIdClaim = httpContext.User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            var result = new ApiResult<object>
            {
                Success = false,
                Message = "Invalid token. User information not found.",
                Errors = new List<string> { "Token does not contain valid user information." }
            };

            context.Result = new UnauthorizedObjectResult(result);
            return;
        }

        var usernameClaim = httpContext.User.FindFirst("Username")?.Value;
        if (string.IsNullOrEmpty(usernameClaim))
        {
            var result = new ApiResult<object>
            {
                Success = false,
                Message = "Invalid token. Username information not found.",
                Errors = new List<string> { "Token does not contain valid username information." }
            };

            context.Result = new UnauthorizedObjectResult(result);
            return;
        }

        if (MinimumPermission > 0)
        {
            var permissionClaim = httpContext.User.FindFirst("Permission")?.Value;
            if (!int.TryParse(permissionClaim, out int userPermission) || userPermission < MinimumPermission)
            {
                var result = new ApiResult<object>
                {
                    Success = false,
                    Message = "Insufficient permissions to access this resource.",
                    Errors = new List<string> { $"Minimum permission level {MinimumPermission} required." }
                };

                context.Result = new ObjectResult(result) { StatusCode = StatusCodes.Status403Forbidden };
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}
