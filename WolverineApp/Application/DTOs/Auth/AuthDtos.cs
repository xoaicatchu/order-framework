namespace WolverineApp.Application.DTOs.Auth;

public record LoginRequest(
    string Username,
    string? Role = "Manager",
    string? TenantId = "default-tenant"
);

public record TokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresInSeconds,
    string UserId,
    string Role,
    string TenantId
);
