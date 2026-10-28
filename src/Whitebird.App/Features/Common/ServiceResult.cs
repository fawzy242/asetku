using System.Text.Json.Serialization;

namespace Whitebird.App.Features.Common;

public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? ErrorType { get; set; }

    /// <summary>
    /// Structured, per-field validation errors. Populated by
    /// <c>ControllerHelper.HandleModelState</c> when ModelState is invalid.
    /// Null for all other responses (and suppressed from JSON when null).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, List<string>>? FieldErrors { get; set; }

    public static ServiceResult<T> Success(T data, string? message = null) => new() { IsSuccess = true, Data = data, Message = message ?? "Operation completed successfully" };
    public static ServiceResult<T> NotFound(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Resource not found", Errors = new() { error }, ErrorType = "NotFound" };
    public static ServiceResult<T> Conflict(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Resource conflict", Errors = new() { error }, ErrorType = "Conflict" };
    public static ServiceResult<T> BadRequest(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Bad request", Errors = new() { error }, ErrorType = "BadRequest" };
    public static ServiceResult<T> BadRequest(List<string> errors, string? message = null) => new() { IsSuccess = false, Message = message ?? "Bad request", Errors = errors, ErrorType = "BadRequest" };
    public static ServiceResult<T> Failure(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Operation failed", Errors = new() { error }, ErrorType = "Error" };
    public static ServiceResult<T> Failure(List<string> errors, string? message = null) => new() { IsSuccess = false, Message = message ?? "Operation failed", Errors = errors, ErrorType = "Error" };
}

public class ServiceResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? ErrorType { get; set; }

    /// <summary>
    /// Structured, per-field validation errors. Populated by
    /// <c>ControllerHelper.HandleModelState</c> when ModelState is invalid.
    /// Null for all other responses (and suppressed from JSON when null).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, List<string>>? FieldErrors { get; set; }

    public static ServiceResult Success(string? message = null) => new() { IsSuccess = true, Message = message ?? "Operation completed successfully" };
    public static ServiceResult NotFound(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Resource not found", Errors = new() { error }, ErrorType = "NotFound" };
    public static ServiceResult Conflict(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Resource conflict", Errors = new() { error }, ErrorType = "Conflict" };
    public static ServiceResult BadRequest(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Bad request", Errors = new() { error }, ErrorType = "BadRequest" };
    public static ServiceResult BadRequest(List<string> errors, string? message = null) => new() { IsSuccess = false, Message = message ?? "Bad request", Errors = errors, ErrorType = "BadRequest" };
    public static ServiceResult Failure(string error, string? message = null) => new() { IsSuccess = false, Message = message ?? "Operation failed", Errors = new() { error }, ErrorType = "Error" };
    public static ServiceResult Failure(List<string> errors, string? message = null) => new() { IsSuccess = false, Message = message ?? "Operation failed", Errors = errors, ErrorType = "Error" };
}