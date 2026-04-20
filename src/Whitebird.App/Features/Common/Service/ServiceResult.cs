namespace Whitebird.App.Features.Common.Service;

public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? ErrorType { get; set; }

    public static ServiceResult<T> Success(T data, string? message = null) => new() { IsSuccess = true, Data = data, Message = message ?? "Operation completed successfully" };
    public static ServiceResult<T> NotFound(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Resource not found", Errors = new() { error }, ErrorType = "NotFound" };
    public static ServiceResult<T> Conflict(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Resource conflict", Errors = new() { error }, ErrorType = "Conflict" };
    public static ServiceResult<T> BadRequest(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Bad request", Errors = new() { error }, ErrorType = "BadRequest" };
    public static ServiceResult<T> Failure(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Operation failed", Errors = new() { error }, ErrorType = "Error" };
    public static ServiceResult<T> Failure(List<string> errors, string? message = null) => new() { IsSuccess = false, Message = message ?? "Operation failed", Errors = errors, ErrorType = "Error" };
}

public class ServiceResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? ErrorType { get; set; }

    public static ServiceResult Success(string? message = null) => new() { IsSuccess = true, Message = message ?? "Operation completed successfully" };
    public static ServiceResult NotFound(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Resource not found", Errors = new() { error }, ErrorType = "NotFound" };
    public static ServiceResult Conflict(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Resource conflict", Errors = new() { error }, ErrorType = "Conflict" };
    public static ServiceResult BadRequest(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Bad request", Errors = new() { error }, ErrorType = "BadRequest" };
    public static ServiceResult Failure(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Operation failed", Errors = new() { error }, ErrorType = "Error" };
    public static ServiceResult Failure(List<string> errors, string? message = null) => new() { IsSuccess = false, Message = message ?? "Operation failed", Errors = errors, ErrorType = "Error" };
}