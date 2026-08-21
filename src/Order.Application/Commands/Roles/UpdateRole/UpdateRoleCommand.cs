using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Authorization;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Roles;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Application.Commands.Roles.UpdateRole;

public record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description,
    List<string> Permissions
) : ICommand<RoleDto>;

public class UpdateRoleCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPermissionService _permissionService;

    public UpdateRoleCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _permissionService = permissionService;
    }

    public async Task<RoleDto> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
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

        var role = await _unitOfWork.GetRepository<AppRole>().Query(tracking: true)
            .Include(r => r.Permissions)
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Id == command.Id && r.TenantId == tenantId, cancellationToken);

        if (role is null)
        {
            throw new KeyNotFoundException($"Role with ID '{command.Id}' not found.");
        }

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("System default roles cannot be modified.");
        }

        role.Update(name, command.Description);

        _unitOfWork.GetRepository<AppRolePermission>().DeleteRange(role.Permissions);
        role.Permissions.Clear();

        foreach (var code in permissionCodes)
        {
            var newPerm = new AppRolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = role.Id,
                PermissionCode = code,
                TenantId = tenantId
            };
            role.Permissions.Add(newPerm);
            await _unitOfWork.GetRepository<AppRolePermission>().AddAsync(newPerm, cancellationToken);
        }

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
