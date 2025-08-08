using Microsoft.AspNetCore.Mvc;
using Api.Extensions;
using Api.Common;

namespace Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected string? CurrentUserId => User.GetUserId(HttpContext.RequestServices);
    protected string? CurrentUsername => User.GetUsername(HttpContext.RequestServices);
    
    protected bool IsAuthenticated => User.IsAuthenticated();

    protected IActionResult Success<T>(T data, string? message = null)
    {
        return Ok(ApiResult<T>.Ok(data, message));
    }

    protected IActionResult Success(string? message = null)
    {
        return Ok(ApiResult.Ok(message));
    }

    protected IActionResult Failure(string message, int statusCode = 400)
    {
        var result = ApiResult.Fail(message);
        return StatusCode(statusCode, result);
    }

    protected IActionResult Failure(string message, int statusCode, List<string> errors)
    {
        var result = ApiResult.Fail(message, errors);
        return StatusCode(statusCode, result);
    }

    protected IActionResult Unauthorized(string message = "Unauthorized access")
    {
        return base.Unauthorized(ApiResult.Fail(message));
    }

    protected IActionResult NotFound(string message = "Resource not found")
    {
        return base.NotFound(ApiResult.Fail(message));
    }

    protected IActionResult BadRequest(string message)
    {
        return base.BadRequest(ApiResult.Fail(message));
    }
}
