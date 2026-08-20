using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Roles;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Application.Queries.Roles.GetPermissionsMatrix;

public record GetPermissionsMatrixQuery : IQuery<PermissionMatrixDto>;

public class GetPermissionsMatrixQueryHandler
{
    private readonly ApplicationDbContext _dbContext;

    public GetPermissionsMatrixQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PermissionMatrixDto> Handle(GetPermissionsMatrixQuery query, CancellationToken cancellationToken)
    {
        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .Where(p => !p.IsSystem) // Bỏ qua quyền root hệ thống khi vẽ ma trận đơn vị
            .ToListAsync(cancellationToken);

        // 1. Tập hợp các Action làm Cột (Columns) theo thứ tự chuẩn
        var standardActionOrder = new List<string> { "Read", "Create", "Update", "Delete", "Cancel", "Approve", "Lock", "Assign" };
        var presentActions = permissions.Select(p => p.Action).Distinct().ToList();

        var sortedColumns = standardActionOrder
            .Where(presentActions.Contains)
            .Concat(presentActions.Except(standardActionOrder))
            .Select(a => new MatrixColumnDto(a))
            .ToList();

        // 2. Nhóm theo Module & Resource làm Dòng (Rows)
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
