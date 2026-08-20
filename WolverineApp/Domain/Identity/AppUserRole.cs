using WolverineApp.Domain.Common;

namespace WolverineApp.Domain.Identity;

public class AppUserRole : BaseEntity, IMultiTenant
{
    public string UserId { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public AppRole Role { get; set; } = null!;
    public string TenantId { get; set; } = string.Empty;
}
