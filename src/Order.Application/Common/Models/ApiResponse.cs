using System.Diagnostics.CodeAnalysis;

namespace WolverineApp.Application.Common.Models;

[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "The response factory preserves the existing typed API contract.")]
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Code { get; set; } = "SUCCESS";
    public string? Message { get; set; }
    public T? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Code = "SUCCESS",
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Created(T data, string? message = null) => new()
    {
        Success = true,
        Code = "CREATED",
        Message = message ?? "Created successfully",
        Data = data
    };

    public static ApiResponse<T> Fail(string message, string code = "ERROR", Dictionary<string, string[]>? errors = null) => new()
    {
        Success = false,
        Code = code,
        Message = message,
        Errors = errors
    };

    public static ApiResponse<T> RequireConfirmation(string confirmMessage, T? contextData = default) => new()
    {
        Success = false,
        Code = "REQUIRES_CONFIRMATION",
        Message = confirmMessage,
        Data = contextData
    };
}
