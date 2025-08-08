namespace Api.Common;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResult<T> Ok(T data, string? message = null)
    {
        return new ApiResult<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResult<T> Fail(string message, List<string>? errors = null)
    {
        return new ApiResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

public class ApiResult : ApiResult<object>
{
    public static ApiResult Ok(string? message = null)
    {
        return new ApiResult
        {
            Success = true,
            Message = message
        };
    }

    public static new ApiResult Fail(string message, List<string>? errors = null)
    {
        return new ApiResult
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
