namespace WolverineApp.Application.Common.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string userId, string tenantId, string permissionCode, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserPermissionsAsync(string userId, string tenantId, CancellationToken cancellationToken = default);
    Task InvalidateUserPermissionsCacheAsync(string userId, string tenantId, CancellationToken cancellationToken = default);
    Task InvalidateTenantPermissionsCacheAsync(string tenantId, CancellationToken cancellationToken = default);
}
