using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Identity;
using WolverineApp.Infrastructure.Persistence.Models;

namespace WolverineApp.Application.Commands.Roles.AssignUserRoles;

public record AssignUserRolesCommand(
    string UserId,
    List<Guid> RoleIds
) : ICommand<bool>;

public class AssignUserRolesCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPermissionService _permissionService;

    public AssignUserRolesCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _permissionService = permissionService;
    }

    public async Task<bool> Handle(AssignUserRolesCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        var normalizedUserId = command.UserId.Trim();

        var roleRepository = _unitOfWork.GetRepository<AppRole>();
        var userRoleRepository = _unitOfWork.GetRepository<AppUserRole>();
        var membershipRepository = _unitOfWork.GetRepository<TenantMembershipRecord>();

        var isTenantMember = await membershipRepository.Query()
            .AnyAsync(m => m.UserId == normalizedUserId
                           && m.TenantId == tenantId
                           && m.IsActive, cancellationToken);

        if (!isTenantMember)
        {
            throw new InvalidOperationException("The user is not an active member of the current tenant.");
        }

        var validRoles = await roleRepository.Query()
            .Where(r => command.RoleIds.Contains(r.Id)
                        && r.TenantId == tenantId
                        && !r.IsDeleted
                        && !r.IsSystemRole)
            .ToListAsync(cancellationToken);

        if (validRoles.Count != command.RoleIds.Distinct().Count())
        {
            throw new InvalidOperationException("One or more selected roles are invalid for the current tenant.");
        }

        var oldUserRoles = await userRoleRepository.Query(tracking: true)
            .Where(ur => ur.UserId == normalizedUserId && ur.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        userRoleRepository.DeleteRange(oldUserRoles);

        foreach (var roleId in command.RoleIds)
        {
            await userRoleRepository.AddAsync(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = normalizedUserId,
                RoleId = roleId,
                TenantId = tenantId
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _permissionService.InvalidateUserPermissionsCacheAsync(normalizedUserId, tenantId, cancellationToken);

        return true;
    }
}
