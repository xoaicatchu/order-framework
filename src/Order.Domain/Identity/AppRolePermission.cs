using System.Diagnostics.CodeAnalysis;
using WolverineApp.Domain.Common;

namespace WolverineApp.Domain.Identity;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Permission is a domain concept and the public name is retained for data compatibility.")]
public class AppRolePermission : BaseEntity, IMultiTenant
{
    public Guid RoleId { get; set; }
    public AppRole Role { get; set; } = null!;
    public string PermissionCode { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}
