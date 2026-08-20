using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Roles;
using WolverineApp.Domain.Identity;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Application.Commands.Roles.UpdateRole;

public record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description,
    List<string> Permissions
) : ICommand<RoleDto>;

public class UpdateRoleCommandHandler
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPermissionService _permissionService;

    public UpdateRoleCommandHandler(
        ApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _permissionService = permissionService;
    }

    public async Task<RoleDto> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;

        var role = await _dbContext.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (role is null)
        {
            throw new KeyNotFoundException($"Role with ID '{command.Id}' not found.");
        }

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("System default roles cannot be modified.");
        }

        role.Update(command.Name, command.Description);

        _dbContext.RolePermissions.RemoveRange(role.Permissions);
        role.Permissions.Clear();

        foreach (var code in command.Permissions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var newPerm = new AppRolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = role.Id,
                PermissionCode = code,
                TenantId = tenantId
            };
            role.Permissions.Add(newPerm);
            await _dbContext.RolePermissions.AddAsync(newPerm, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _permissionService.InvalidateTenantPermissionsCacheAsync(tenantId, cancellationToken);

        return new RoleDto(
            role.Id,
            role.Name,
            role.Description,
            role.TenantId,
            role.IsSystemRole,
            role.Permissions.Select(p => p.PermissionCode).ToList(),
            role.CreatedAt
        );
    }
}
