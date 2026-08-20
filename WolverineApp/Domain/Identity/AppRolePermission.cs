using WolverineApp.Domain.Common;

namespace WolverineApp.Domain.Identity;

public class AppRolePermission : BaseEntity, IMultiTenant
{
    public Guid RoleId { get; set; }
    public AppRole Role { get; set; } = null!;
    public string PermissionCode { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}
