using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Roles;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Application.Queries.Roles.GetPermissions;

public record GetPermissionsQuery : IQuery<List<PermissionDto>>;

public class GetPermissionsQueryHandler
{
    private readonly ApplicationDbContext _dbContext;

    public GetPermissionsQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PermissionDto>> Handle(GetPermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);

        return permissions.Select(p => new PermissionDto(
            p.Id,
            p.Code,
            p.Module,
            p.Resource,
            p.Action,
            p.IsAutoDiscovered
        )).ToList();
    }
}
