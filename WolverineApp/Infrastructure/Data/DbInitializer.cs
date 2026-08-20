using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WolverineApp.Domain.Identity;
using WolverineApp.Infrastructure.Auth;

namespace WolverineApp.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedInitialDataAsync(ApplicationDbContext context, ILogger logger)
    {
        await PermissionDiscoveryService.DiscoverAndSyncPermissionsAsync(context, typeof(DbInitializer).Assembly, logger);

        var systemRootRole = await context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.IsSystemRole && r.Name == "SystemRootAdmin");

        if (systemRootRole is null)
        {
            systemRootRole = AppRole.Create("SystemRootAdmin", "System Root Super Admin Role", "system", isSystemRole: true);
            systemRootRole.SetPermissions([PermissionDiscoveryService.RootPermissionCode]);
            await context.Roles.AddAsync(systemRootRole);
        }

        const string defaultTenant = "default-tenant";

        var adminDonViRole = await context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.TenantId == defaultTenant && r.Name == "AdminDonVi");

        if (adminDonViRole is null)
        {
            adminDonViRole = AppRole.Create("AdminDonVi", "Tenant Unit Administrator", defaultTenant);
            adminDonViRole.SetPermissions([
                "Orders:Read",
                "Orders:Create",
                "Orders:Update",
                "Orders:Cancel",
                "Roles:Read",
                "Roles:Create",
                "Roles:Update",
                "Roles:Delete",
                "Roles:Assign",
                "AuditLogs:Read",
                "Reports:Read",
                "Reports:Export"
            ]);
            await context.Roles.AddAsync(adminDonViRole);

            context.UserRoles.Add(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = "alice_manager",
                RoleId = adminDonViRole.Id,
                TenantId = defaultTenant
            });
        }

        var operatorRole = await context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.TenantId == defaultTenant && r.Name == "NhanVienVanHanh");

        if (operatorRole is null)
        {
            operatorRole = AppRole.Create("NhanVienVanHanh", "Order Operations Staff", defaultTenant);
            operatorRole.SetPermissions([
                "Orders:Read",
                "Orders:Create",
                "Orders:Update"
            ]);
            await context.Roles.AddAsync(operatorRole);

            context.UserRoles.Add(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = "bob_operator",
                RoleId = operatorRole.Id,
                TenantId = defaultTenant
            });
        }

        const string tenantB = "tenant-b";
        var tenantBAdminRole = await context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.TenantId == tenantB && r.Name == "AdminDonVi");

        if (tenantBAdminRole is null)
        {
            tenantBAdminRole = AppRole.Create("AdminDonVi", "Tenant B Administrator", tenantB);
            tenantBAdminRole.SetPermissions([
                "Orders:Read",
                "Orders:Create",
                "Orders:Update",
                "Orders:Cancel",
                "Roles:Read",
                "Roles:Create",
                "Roles:Update",
                "Roles:Delete",
                "Roles:Assign",
                "AuditLogs:Read",
                "Reports:Read",
                "Reports:Export"
            ]);
            await context.Roles.AddAsync(tenantBAdminRole);

            context.UserRoles.Add(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = "charlie_tenant_b",
                RoleId = tenantBAdminRole.Id,
                TenantId = tenantB
            });
        }

        await context.SaveChangesAsync();
    }
}
