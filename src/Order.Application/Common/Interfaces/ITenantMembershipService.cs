namespace WolverineApp.Application.Common.Interfaces;

public interface ITenantMembershipService
{
    Task<bool> IsActiveMemberAsync(string userId, string tenantId, CancellationToken cancellationToken = default);
}
