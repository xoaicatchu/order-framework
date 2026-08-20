using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Roles;
using WolverineApp.Domain.Identity;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Application.Commands.Roles.CreateRole;

public record CreateRoleCommand(
    string Name,
    string? Description,
    List<string> Permissions
) : ICommand<RoleDto>;

public class CreateRoleCommandHandler
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPermissionService _permissionService;

    public CreateRoleCommandHandler(
        ApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _permissionService = permissionService;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;

        var existing = await _dbContext.Roles
            .AnyAsync(r => r.TenantId == tenantId && r.Name == command.Name.Trim(), cancellationToken);

        if (existing)
        {
            throw new InvalidOperationException($"Role '{command.Name}' already exists in tenant '{tenantId}'.");
        }

        var role = AppRole.Create(command.Name, command.Description, tenantId);
        role.SetPermissions(command.Permissions);

        await _dbContext.Roles.AddAsync(role, cancellationToken);
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
