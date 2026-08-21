using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Roles;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Application.Queries.Roles.GetPermissionsMatrix;

public record GetPermissionsMatrixQuery : IQuery<PermissionMatrixDto>;

public class GetPermissionsMatrixQueryHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPermissionsMatrixQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PermissionMatrixDto> Handle(GetPermissionsMatrixQuery query, CancellationToken cancellationToken)
    {
        var permissions = await _unitOfWork.GetRepository<AppPermission>().Query()
            .Where(p => !p.IsSystem)
            .ToListAsync(cancellationToken);

        var standardActionOrder = new List<string> { "Read", "Create", "Update", "Delete", "Cancel", "Approve", "Lock", "Assign" };
        var presentActions = permissions.Select(p => p.Action).Distinct().ToList();

        var sortedColumns = standardActionOrder
            .Where(presentActions.Contains)
            .Concat(presentActions.Except(standardActionOrder))
            .Select(a => new MatrixColumnDto(a))
            .ToList();

        var rows = permissions
            .GroupBy(p => new { p.Module, p.Resource })
            .OrderBy(g => g.Key.Module)
            .ThenBy(g => g.Key.Resource)
            .Select(g => new MatrixRowDto(
                g.Key.Module,
                g.Key.Resource,
                g.ToDictionary(p => p.Action, p => p.Code)
            ))
            .ToList();

        return new PermissionMatrixDto(sortedColumns, rows);
    }
}
