using System.Text.Json;
using FluentValidation;
using WolverineApp.Application.Common.Exceptions;
using WolverineApp.Application.Common.Models;

namespace WolverineApp.Infrastructure.Middleware;

public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;

    public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessConfirmationException ex)
        {
            _logger.LogInformation("Action requires user confirmation: {Message}", ex.Message);
            var response = ApiResponse<object>.RequireConfirmation(ex.Message, ex.ContextData);
            await WriteResponseAsync(context, StatusCodes.Status200OK, response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Errors}", ex.Errors);
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Authorization failure: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status403Forbidden, ApiResponse<object>.Fail("Forbidden.", "FORBIDDEN"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status400BadRequest, ApiResponse<object>.Fail(ex.Message, "BAD_REQUEST"));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status404NotFound, ApiResponse<object>.Fail(ex.Message, "NOT_FOUND"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled server error: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("Đã có lỗi hệ thống xảy ra. Vui lòng thử lại sau.", "INTERNAL_ERROR"));
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        var validationErrors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var firstErrorMessage = exception.Errors.FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ.";

        var apiResponse = ApiResponse<object>.Fail(
            message: firstErrorMessage,
            code: "VALIDATION_ERROR",
            errors: validationErrors
        );

        await WriteResponseAsync(context, StatusCodes.Status400BadRequest, apiResponse);
    }

    private static async Task WriteResponseAsync(HttpContext context, int statusCode, ApiResponse<object> response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        await context.Response.WriteAsync(json);
    }
}
