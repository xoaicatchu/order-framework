using WolverineApp.Domain.Common;

namespace WolverineApp.Domain.Identity;

public class AppRole : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; } = false;

    public List<AppRolePermission> Permissions { get; set; } = [];
    public List<AppUserRole> UserRoles { get; set; } = [];

    public static AppRole Create(string name, string? description, string tenantId, bool isSystemRole = false)
    {
        return new AppRole
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            TenantId = tenantId,
            IsSystemRole = isSystemRole
        };
    }

    public void Update(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
    }

    public void SetPermissions(IEnumerable<string> permissionCodes)
    {
        Permissions.Clear();
        foreach (var code in permissionCodes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            Permissions.Add(new AppRolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = Id,
                PermissionCode = code,
                TenantId = TenantId
            });
        }
    }
}
