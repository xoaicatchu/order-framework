using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Authorization;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Roles;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Application.Commands.Roles.CreateRole;

public record CreateRoleCommand(
    string Name,
    string? Description,
    List<string> Permissions
) : ICommand<RoleDto>;

public class CreateRoleCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPermissionService _permissionService;

    public CreateRoleCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _permissionService = permissionService;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        var name = command.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            throw new InvalidOperationException("Role name is required and must be at most 100 characters.");
        }

        var permissionCodes = command.Permissions
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (permissionCodes.Contains(PermissionCodes.Root, StringComparer.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Tenant roles cannot contain System:Root.");
        }

        var knownPermissions = await _unitOfWork.GetRepository<AppPermission>().Query()
            .Where(p => permissionCodes.Contains(p.Code))
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        if (knownPermissions.Count != permissionCodes.Count)
        {
            throw new InvalidOperationException("One or more permissions are not registered.");
        }

        var existing = await _unitOfWork.GetRepository<AppRole>().Query()
            .AnyAsync(r => r.TenantId == tenantId && r.Name == name, cancellationToken);

        if (existing)
        {
            throw new InvalidOperationException($"Role '{command.Name}' already exists in tenant '{tenantId}'.");
        }

        var role = AppRole.Create(name, command.Description, tenantId);
        role.SetPermissions(permissionCodes);

        await _unitOfWork.GetRepository<AppRole>().AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
