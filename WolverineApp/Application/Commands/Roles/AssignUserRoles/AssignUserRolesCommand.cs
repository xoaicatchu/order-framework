using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Identity;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Application.Commands.Roles.AssignUserRoles;

public record AssignUserRolesCommand(
    string UserId,
    List<Guid> RoleIds
) : ICommand<bool>;

public class AssignUserRolesCommandHandler
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPermissionService _permissionService;

    public AssignUserRolesCommandHandler(
        ApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _permissionService = permissionService;
    }

    public async Task<bool> Handle(AssignUserRolesCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        var normalizedUserId = command.UserId.Trim();

        var validRoles = await _dbContext.Roles
            .Where(r => command.RoleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        if (validRoles.Count != command.RoleIds.Count)
        {
            throw new InvalidOperationException("One or more selected roles are invalid for the current tenant.");
        }

        var oldUserRoles = await _dbContext.UserRoles
            .Where(ur => ur.UserId == normalizedUserId && ur.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        _dbContext.UserRoles.RemoveRange(oldUserRoles);

        foreach (var roleId in command.RoleIds)
        {
            _dbContext.UserRoles.Add(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = normalizedUserId,
                RoleId = roleId,
                TenantId = tenantId
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _permissionService.InvalidateUserPermissionsCacheAsync(normalizedUserId, tenantId, cancellationToken);

        return true;
    }
}
