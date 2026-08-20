using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    public const string RootPermissionCode = "System:Root";
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
                _logger.LogDebug("Resolving dynamic permissions for User: {UserId} in Tenant: {TenantId}", userId, tenantId);

                return await _dbContext.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
                    .Join(
                        _dbContext.RolePermissions.AsNoTracking(),
                        ur => ur.RoleId,
                        rp => rp.RoleId,
                        (ur, rp) => rp.PermissionCode)
                    .Distinct()
                    .ToListAsync(ct);
            },
            expiration: TimeSpan.FromMinutes(15),
            tags: [tenantTag],
            cancellationToken: cancellationToken);
    }

    public async Task<bool> HasPermissionAsync(string userId, string tenantId, string permissionCode, CancellationToken cancellationToken = default)
    {
        var userPermissions = await GetUserPermissionsAsync(userId, tenantId, cancellationToken);

        if (userPermissions.Contains(RootPermissionCode, StringComparer.OrdinalIgnoreCase))
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
