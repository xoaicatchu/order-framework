using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Identity;
using WolverineApp.Infrastructure.Persistence.Models;

namespace WolverineApp.Infrastructure.Identity;

public class PermissionService : IPermissionService
{
    private const string RootPermissionCode = WolverineApp.Application.Common.Authorization.PermissionCodes.Root;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<PermissionService> logger)
    {
        _unitOfWork = unitOfWork;
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

                var rootRole = _unitOfWork.GetRepository<AppUserRole>().Query()
                    .AsNoTracking()
                    .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
                    .Join(
                        _unitOfWork.GetRepository<AppRole>().Query(ignoreFilters: true),
                        ur => ur.RoleId,
                        role => role.Id,
                        (_, role) => role)
                    .AnyAsync(role => role.IsSystemRole && role.Permissions.Any(p => p.PermissionCode == RootPermissionCode), ct);

                if (await rootRole)
                {
                    return await _unitOfWork.GetRepository<AppPermission>().Query()
                        .AsNoTracking()
                        .Select(permission => permission.Code)
                        .ToListAsync(ct);
                }

                var roleIds = _unitOfWork.GetRepository<AppUserRole>().Query()
                    .AsNoTracking()
                    .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
                    .Join(
                        _unitOfWork.GetRepository<TenantMembershipRecord>().Query()
                            .Where(m => m.UserId == userId && m.TenantId == tenantId && m.IsActive),
                        ur => new { ur.UserId, ur.TenantId },
                        membership => new { membership.UserId, membership.TenantId },
                        (ur, _) => ur)
                    .Join(
                        _unitOfWork.GetRepository<AppRole>().Query().Where(r => r.TenantId == tenantId && !r.IsDeleted && !r.IsSystemRole),
                        ur => ur.RoleId,
                        role => role.Id,
                        (_, role) => role.Id);

                return await _unitOfWork.GetRepository<AppRolePermission>().Query()
                    .Where(rp => rp.TenantId == tenantId && roleIds.Contains(rp.RoleId))
                    .Join(
                        _unitOfWork.GetRepository<AppPermission>().Query(),
                        rp => rp.PermissionCode,
                        permission => permission.Code,
                        (_, permission) => permission.Code)
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
