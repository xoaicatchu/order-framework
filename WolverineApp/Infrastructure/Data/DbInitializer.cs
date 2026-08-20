using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WolverineApp.Domain.Identity;
using WolverineApp.Infrastructure.Auth;

namespace WolverineApp.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedInitialDataAsync(ApplicationDbContext context, ILogger logger)
    {
        // 1. Tự động quét Controller/Action và đồng bộ toàn bộ Permissions vào Database
        await PermissionDiscoveryService.DiscoverAndSyncPermissionsAsync(context, typeof(DbInitializer).Assembly, logger);

        // 2. Seed vai trò System Role (Root Admin)
        var systemRootRole = await context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.IsSystemRole && r.Name == "SystemRootAdmin");

        if (systemRootRole is null)
        {
            systemRootRole = AppRole.Create("SystemRootAdmin", "System Root Super Admin Role", "system", isSystemRole: true);
            systemRootRole.SetPermissions([PermissionDiscoveryService.RootPermissionCode]);
            await context.Roles.AddAsync(systemRootRole);
            logger.LogInformation("🌱 [Seed] Seeded System Root Role.");
        }

        // 3. Seed vai trò mẫu cho Default Tenant (Admin Đơn Vị & Nhân Viên Vận Hành)
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
                "AuditLogs:Read"
            ]);
            await context.Roles.AddAsync(adminDonViRole);

            // Gán role Admin cho alice_manager
            context.UserRoles.Add(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = "alice_manager",
                RoleId = adminDonViRole.Id,
                TenantId = defaultTenant
            });
            logger.LogInformation("🌱 [Seed] Seeded AdminDonVi role and assigned to alice_manager.");
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

            // Gán role Operator cho bob_operator
            context.UserRoles.Add(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = "bob_operator",
                RoleId = operatorRole.Id,
                TenantId = defaultTenant
            });
            logger.LogInformation("🌱 [Seed] Seeded NhanVienVanHanh role and assigned to bob_operator.");
        }

        // 4. Seed vai trò mẫu cho Tenant B
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
                "AuditLogs:Read"
            ]);
            await context.Roles.AddAsync(tenantBAdminRole);

            context.UserRoles.Add(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = "charlie_tenant_b",
                RoleId = tenantBAdminRole.Id,
                TenantId = tenantB
            });
            logger.LogInformation("🌱 [Seed] Seeded AdminDonVi role for Tenant B and assigned to charlie_tenant_b.");
        }

        await context.SaveChangesAsync();
    }
}
