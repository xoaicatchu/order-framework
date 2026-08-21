using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Infrastructure.Persistence.Models;

namespace WolverineApp.Infrastructure.Identity;

public sealed class TenantMembershipService : ITenantMembershipService
{
    private readonly IUnitOfWork _unitOfWork;

    public TenantMembershipService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<bool> IsActiveMemberAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
        => _unitOfWork.GetRepository<TenantMembershipRecord>().Query()
            .AnyAsync(m => m.UserId == userId && m.TenantId == tenantId && m.IsActive, cancellationToken);
}
