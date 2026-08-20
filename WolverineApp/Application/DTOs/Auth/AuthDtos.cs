namespace WolverineApp.Application.DTOs.Auth;

public record LoginRequest(
    string Username,
    string? TenantId = "default-tenant",
    bool IsRoot = false,
    List<string>? Permissions = null
);

public record TokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresInSeconds,
    string UserId,
    string TenantId,
    bool IsRoot,
    List<string> Permissions
);
