using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Application.Commands.Roles.DeleteRole;

public record DeleteRoleCommand(Guid Id) : ICommand<bool>;

public class DeleteRoleCommandHandler
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPermissionService _permissionService;

    public DeleteRoleCommandHandler(
        ApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _permissionService = permissionService;
    }

    public async Task<bool> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;

        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (role is null)
        {
            throw new KeyNotFoundException($"Role with ID '{command.Id}' not found.");
        }

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("System default roles cannot be deleted.");
        }

        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _permissionService.InvalidateTenantPermissionsCacheAsync(tenantId, cancellationToken);

        return true;
    }
}
