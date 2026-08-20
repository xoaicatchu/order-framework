using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Roles;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Application.Queries.Roles.GetRoles;

public record GetRolesQuery : IQuery<List<RoleDto>>;

public class GetRolesQueryHandler
{
    private readonly ApplicationDbContext _dbContext;

    public GetRolesQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery query, CancellationToken cancellationToken)
    {
        // Global query filter tự động lọc vai trò theo TenantId hiện tại của người dùng
        var roles = await _dbContext.Roles
            .AsNoTracking()
            .Include(r => r.Permissions)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return roles.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Description,
            r.TenantId,
            r.IsSystemRole,
            r.Permissions.Select(p => p.PermissionCode).ToList(),
            r.CreatedAt
        )).ToList();
    }
}
