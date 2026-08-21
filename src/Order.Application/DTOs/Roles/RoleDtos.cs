namespace WolverineApp.Application.DTOs.Roles;

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    string TenantId,
    bool IsSystemRole,
    List<string> Permissions,
    DateTime CreatedAt
);

public record CreateRoleRequest(
    string Name,
    string? Description,
    List<string> Permissions
);

public record UpdateRoleRequest(
    string Name,
    string? Description,
    List<string> Permissions
);

public record AssignUserRolesRequest(
    string UserId,
    List<Guid> RoleIds
);
