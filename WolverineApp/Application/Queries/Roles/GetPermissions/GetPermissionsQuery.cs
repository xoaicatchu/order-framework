using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Application.Queries.Roles.GetPermissions;

public record GetPermissionsQuery : IQuery<List<PermissionDefinition>>;

public class GetPermissionsQueryHandler
{
    public Task<List<PermissionDefinition>> Handle(GetPermissionsQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(SystemPermissions.All);
    }
}
