using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Roles;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Application.Queries.Roles.GetRoleById;

public record GetRoleByIdQuery(Guid Id) : IQuery<RoleDto>;

public class GetRoleByIdQueryHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleDto> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.GetRepository<AppRole>().Query()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

        if (role is null)
            throw new KeyNotFoundException($"Không tìm thấy vai trò với ID: {query.Id}");

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
