using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Roles;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Application.Queries.Roles.GetPermissions;

public record GetPermissionsQuery : IQuery<List<PermissionDto>>;

public class GetPermissionsQueryHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPermissionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PermissionDto>> Handle(GetPermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissions = await _unitOfWork.GetRepository<AppPermission>().Query()
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
