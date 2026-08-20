using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Identity;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        ApplicationDbContext dbContext,
        ICacheService cacheService,
        ILogger<PermissionService> logger)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user-permissions:{tenantId}:{userId}";
        var tenantTag = $"tenant-permissions:{tenantId}";

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                _logger.LogDebug("🔍 [Dynamic RBAC] Resolving dynamic permissions for User: {UserId} in Tenant: {TenantId}", userId, tenantId);

                // JOIN UserRoles và RolePermissions lấy toàn bộ mã quyền của User trong Tenant
                var permissions = await _dbContext.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
                    .Join(
                        _dbContext.RolePermissions.AsNoTracking(),
                        ur => ur.RoleId,
                        rp => rp.RoleId,
                        (ur, rp) => rp.PermissionCode)
                    .Distinct()
                    .ToListAsync(ct);

                return permissions;
            },
            expiration: TimeSpan.FromMinutes(15),
            tags: [tenantTag],
            cancellationToken: cancellationToken);
    }

    public async Task<bool> HasPermissionAsync(string userId, string tenantId, string permissionCode, CancellationToken cancellationToken = default)
    {
        var userPermissions = await GetUserPermissionsAsync(userId, tenantId, cancellationToken);

        // Nếu có quyền Root (Quản trị tối cao toàn hệ thống) thì cho phép toàn bộ
        if (userPermissions.Contains(SystemPermissions.SystemRoot, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return userPermissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }

    public async Task InvalidateUserPermissionsCacheAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user-permissions:{tenantId}:{userId}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);
    }

    public async Task InvalidateTenantPermissionsCacheAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var tenantTag = $"tenant-permissions:{tenantId}";
        await _cacheService.RemoveByTagAsync(tenantTag, cancellationToken);
    }
}
