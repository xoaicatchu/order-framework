using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Identity;
using WolverineApp.Infrastructure.Auth;
using WolverineApp.Infrastructure.Persistence.Models;

namespace WolverineApp.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SynchronizePermissionsAsync(IUnitOfWork unitOfWork, System.Reflection.Assembly apiAssembly, ILogger logger)
    {
        await PermissionDiscoveryService.DiscoverAndSyncPermissionsAsync(unitOfWork, apiAssembly, logger);
    }

    public static async Task SeedDevelopmentDataAsync(IUnitOfWork unitOfWork, ILogger logger)
    {
        var roleRepository = unitOfWork.GetRepository<AppRole>();
        var templateRepository = unitOfWork.GetRepository<WolverineApp.Domain.Reporting.ReportTemplate>();
        var datasetRepository = unitOfWork.GetRepository<WolverineApp.Domain.Reporting.SemanticDataset>();
        var membershipRepository = unitOfWork.GetRepository<TenantMembershipRecord>();

        var seedMemberships = new[]
        {
            ("alice_manager", "default-tenant"),
            ("bob_operator", "default-tenant"),
            ("charlie_tenant_b", "tenant-b")
        };

        foreach (var (userId, tenantId) in seedMemberships)
        {
            if (!await membershipRepository.Query(ignoreFilters: true)
                    .AnyAsync(m => m.UserId == userId && m.TenantId == tenantId))
            {
                await membershipRepository.AddAsync(new TenantMembershipRecord
                {
                    UserId = userId,
                    TenantId = tenantId,
                    IsActive = true
                });
            }
        }

        var systemRootRole = await roleRepository.Query(ignoreFilters: true)
            .FirstOrDefaultAsync(r => r.IsSystemRole && r.Name == "SystemRootAdmin");

        if (systemRootRole is null)
        {
            systemRootRole = AppRole.Create("SystemRootAdmin", "System Root Super Admin Role", "system", isSystemRole: true);
            systemRootRole.SetPermissions([PermissionDiscoveryService.RootPermissionCode]);
            await roleRepository.AddAsync(systemRootRole);
        }

        const string defaultTenant = "default-tenant";

        var adminDonViRole = await roleRepository.Query(ignoreFilters: true)
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
            adminDonViRole.UserRoles.Add(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = "alice_manager",
                RoleId = adminDonViRole.Id,
                TenantId = defaultTenant
            });
            await roleRepository.AddAsync(adminDonViRole);
        }

        var operatorRole = await roleRepository.Query(ignoreFilters: true)
            .FirstOrDefaultAsync(r => r.TenantId == defaultTenant && r.Name == "NhanVienVanHanh");

        if (operatorRole is null)
        {
            operatorRole = AppRole.Create("NhanVienVanHanh", "Order Operations Staff", defaultTenant);
            operatorRole.SetPermissions([
                "Orders:Read",
                "Orders:Create",
                "Orders:Update"
            ]);
            operatorRole.UserRoles.Add(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = "bob_operator",
                RoleId = operatorRole.Id,
                TenantId = defaultTenant
            });
            await roleRepository.AddAsync(operatorRole);
        }

        const string tenantB = "tenant-b";
        var tenantBAdminRole = await roleRepository.Query(ignoreFilters: true)
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
            tenantBAdminRole.UserRoles.Add(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = "charlie_tenant_b",
                RoleId = tenantBAdminRole.Id,
                TenantId = tenantB
            });
            await roleRepository.AddAsync(tenantBAdminRole);
        }

        // Seed System Default Report Templates
        var invoiceTemplate = await templateRepository.Query(ignoreFilters: true)
            .FirstOrDefaultAsync(t => t.IsSystemDefault && t.Code == "Invoice_A4");

        if (invoiceTemplate is null)
        {
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Reporting", "Templates", "Invoice_A4.liquid");
            if (!File.Exists(templatePath))
            {
                templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Reporting", "Templates", "Invoice_A4.liquid");
            }

            string content = "";
            if (File.Exists(templatePath))
            {
                content = await File.ReadAllTextAsync(templatePath);
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                var systemTemplate = WolverineApp.Domain.Reporting.ReportTemplate.Create(
                    code: "Invoice_A4",
                    name: "Hóa đơn bán hàng & Xuất kho A4 Chuẩn",
                    content: content,
                    tenantId: "system",
                    description: "Mẫu in hóa đơn A4 mặc định của hệ thống",
                    category: "Billing",
                    isSystemDefault: true
                );
                await templateRepository.AddAsync(systemTemplate);
            }
        }

        // Seed System Default Semantic Datasets
        var salesDataset = await datasetRepository.Query(ignoreFilters: true)
            .FirstOrDefaultAsync(d => d.Code == "Sales_Orders_Dataset");

        if (salesDataset is null)
        {
            var fields = new List<WolverineApp.Domain.Reporting.SemanticFieldDefinition>
            {
                new() { Key = "OrderNumber", Label = "Mã đơn hàng", Type = "string", Filterable = true },
                new() { Key = "CustomerName", Label = "Khách hàng / Bệnh nhân", Type = "string", Filterable = true },
                new() { Key = "CustomerEmail", Label = "Email liên hệ", Type = "string", Filterable = false },
                new() { Key = "Status", Label = "Trạng thái", Type = "enum", Filterable = true, EnumValues = ["Created:Đã tạo", "Processing:Đang xử lý", "Completed:Hoàn thành", "Cancelled:Đã hủy"] },
                new() { Key = "OrderTotal", Label = "Tổng tiền đơn hàng", Type = "currency", Filterable = true },
                new() { Key = "ProductName", Label = "Tên hàng hóa / Thuốc", Type = "string", Filterable = true },
                new() { Key = "Sku", Label = "Mã SKU", Type = "string", Filterable = false },
                new() { Key = "Quantity", Label = "Số lượng", Type = "number", Filterable = false },
                new() { Key = "UnitPrice", Label = "Đơn giá", Type = "currency", Filterable = false },
                new() { Key = "ItemTotal", Label = "Thành tiền", Type = "currency", Filterable = true },
                new() { Key = "CreatedAt", Label = "Ngày lập hóa đơn", Type = "date", Filterable = true }
            };

            var fieldsJson = System.Text.Json.JsonSerializer.Serialize(fields);
            const string baseSql = @"
SELECT 
    o.OrderNumber AS OrderNumber,
    o.CustomerName AS CustomerName,
    o.CustomerEmail AS CustomerEmail,
    o.Status AS Status,
    o.TotalAmount AS OrderTotal,
    oi.ProductName AS ProductName,
    oi.Sku AS Sku,
    oi.Quantity AS Quantity,
    oi.UnitPrice AS UnitPrice,
    oi.Total AS ItemTotal,
    o.CreatedAt AS CreatedAt
FROM Orders o
INNER JOIN OrderItems oi ON o.Id = oi.OrderId
WHERE o.TenantId = @TenantId
  AND o.IsDeleted = FALSE
  AND oi.IsDeleted = FALSE";

            var newDataset = WolverineApp.Domain.Reporting.SemanticDataset.Create(
                code: "Sales_Orders_Dataset",
                name: "Dữ liệu Hóa đơn Bán hàng & Dịch vụ Khám chữa bệnh",
                category: "Tài chính & Thu ngân",
                description: "Nguồn dữ liệu chi tiết hóa đơn, dịch vụ khám, thuốc và thành tiền",
                fieldsMetadataJson: fieldsJson,
                baseQuerySql: baseSql
            );

            await datasetRepository.AddAsync(newDataset);
        }

        await unitOfWork.SaveChangesAsync();
    }
}
