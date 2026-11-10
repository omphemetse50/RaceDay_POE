namespace RaceDay.Web.Services;

/// <summary>
/// Wraps every API call outcome so controllers never have to deal with raw
/// exceptions or HTTP status codes directly -- they just check Success and,
/// on failure, show Error to the user via TempData/ModelState instead of a
/// crash or a raw stack trace.
///
/// Named ApiCallResult (not ApiResult) specifically to avoid colliding with
/// RaceDay.Web.Models.ApiResult, which represents a race result DTO.
/// </summary>
public class ApiCallResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }

    public static ApiCallResult<T> Ok(T data, int statusCode = 200) => new() { Success = true, Data = data, StatusCode = statusCode };
    public static ApiCallResult<T> Fail(string error, int statusCode = 0) => new() { Success = false, Error = error, StatusCode = statusCode };
}
