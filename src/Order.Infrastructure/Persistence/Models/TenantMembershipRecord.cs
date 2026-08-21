using WolverineApp.Domain.Common;

namespace WolverineApp.Infrastructure.Persistence.Models;

public class TenantMembershipRecord : IMultiTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
