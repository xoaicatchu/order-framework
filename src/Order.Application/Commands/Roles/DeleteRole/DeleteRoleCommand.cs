using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Application.Commands.Roles.DeleteRole;

public record DeleteRoleCommand(Guid Id) : ICommand<bool>;

public class DeleteRoleCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPermissionService _permissionService;

    public DeleteRoleCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _permissionService = permissionService;
    }

    public async Task<bool> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;

        var roleRepository = _unitOfWork.GetRepository<AppRole>();
        var rolePermissionRepository = _unitOfWork.GetRepository<AppRolePermission>();
        var userRoleRepository = _unitOfWork.GetRepository<AppUserRole>();

        var role = await roleRepository.Query(tracking: true, ignoreFilters: true)
            .FirstOrDefaultAsync(r => r.Id == command.Id && r.TenantId == tenantId && !r.IsDeleted, cancellationToken);

        if (role is null)
        {
            throw new KeyNotFoundException($"Role with ID '{command.Id}' not found.");
        }

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("System default roles cannot be deleted.");
        }

        var affectedUserIds = await userRoleRepository.Query()
            .Where(ur => ur.RoleId == role.Id && ur.TenantId == tenantId)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var rolePermissions = await rolePermissionRepository.Query(tracking: true)
            .Where(rp => rp.RoleId == role.Id && rp.TenantId == tenantId)
            .ToListAsync(cancellationToken);
        var userRoles = await userRoleRepository.Query(tracking: true)
            .Where(ur => ur.RoleId == role.Id && ur.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        rolePermissionRepository.DeleteRange(rolePermissions);
        userRoleRepository.DeleteRange(userRoles);
        roleRepository.Delete(role);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        foreach (var userId in affectedUserIds)
        {
            await _permissionService.InvalidateUserPermissionsCacheAsync(userId, tenantId, cancellationToken);
        }
        await _permissionService.InvalidateTenantPermissionsCacheAsync(tenantId, cancellationToken);

        return true;
    }
}
