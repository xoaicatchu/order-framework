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

        // 1. Kiểm tra các RoleIds có hợp lệ trong tenant không
        var validRoles = await _dbContext.Roles
            .Where(r => command.RoleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        if (validRoles.Count != command.RoleIds.Count)
        {
            throw new InvalidOperationException("Một số vai trò được chọn không hợp lệ hoặc không thuộc đơn vị hiện tại.");
        }

        // 2. Xóa các vai trò cũ của user trong tenant này
        var oldUserRoles = await _dbContext.UserRoles
            .Where(ur => ur.UserId == normalizedUserId && ur.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        _dbContext.UserRoles.RemoveRange(oldUserRoles);

        // 3. Gán các vai trò mới
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

        // 4. Hủy cache quyền của user để áp dụng tức thời
        await _permissionService.InvalidateUserPermissionsCacheAsync(normalizedUserId, tenantId, cancellationToken);

        return true;
    }
}
