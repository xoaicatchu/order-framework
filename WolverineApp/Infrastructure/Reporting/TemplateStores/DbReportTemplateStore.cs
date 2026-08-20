using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Reporting;
using WolverineApp.Domain.Reporting;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Infrastructure.Reporting.TemplateStores;

public class DbReportTemplateStore : IReportTemplateStore
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DbReportTemplateStore> _logger;

    public DbReportTemplateStore(
        IServiceScopeFactory scopeFactory,
        ICacheService cacheService,
        ILogger<DbReportTemplateStore> logger)
    {
        _scopeFactory = scopeFactory;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<string?> GetTemplateContentAsync(string templateCode, string tenantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"report_template:{tenantId}:{templateCode}";

        var cachedContent = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedContent))
        {
            return cachedContent;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 1. Tìm mẫu tùy biến riêng của Tenant này trước
        var tenantTemplate = await db.ReportTemplates
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Code == templateCode && t.IsActive && !t.IsDeleted, cancellationToken);

        if (tenantTemplate != null && !string.IsNullOrWhiteSpace(tenantTemplate.Content))
        {
            await _cacheService.SetAsync(cacheKey, tenantTemplate.Content, TimeSpan.FromHours(2), tags: null, cancellationToken);
            return tenantTemplate.Content;
        }

        // 2. Nếu Tenant chưa tùy biến, fallback về Mẫu Chuẩn Hệ Thống (IsSystemDefault = true)
        var systemDefaultTemplate = await db.ReportTemplates
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.IsSystemDefault && t.Code == templateCode && t.IsActive && !t.IsDeleted, cancellationToken);

        if (systemDefaultTemplate != null && !string.IsNullOrWhiteSpace(systemDefaultTemplate.Content))
        {
            await _cacheService.SetAsync(cacheKey, systemDefaultTemplate.Content, TimeSpan.FromHours(2), tags: null, cancellationToken);
            return systemDefaultTemplate.Content;
        }

        _logger.LogWarning("Template '{TemplateCode}' not found in DB for tenant '{TenantId}' nor as System Default", templateCode, tenantId);
        return null;
    }

    public async Task SaveCustomTemplateAsync(string templateCode, string tenantId, string content, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var existing = await db.ReportTemplates
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Code == templateCode, cancellationToken);

        if (existing != null)
        {
            existing.Content = content;
            existing.IsDeleted = false;
            existing.IsActive = true;
            existing.Version++;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var newTemplate = ReportTemplate.Create(
                code: templateCode,
                name: $"Mẫu in {templateCode}",
                content: content,
                tenantId: tenantId,
                isSystemDefault: false
            );
            await db.ReportTemplates.AddAsync(newTemplate, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        // Xóa Cache để lượt gọi kế tiếp nhận mẫu mới ngay lập tức
        var cacheKey = $"report_template:{tenantId}:{templateCode}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);
        _logger.LogInformation("Saved custom template '{TemplateCode}' for tenant '{TenantId}' to DB and evicted RAM cache", templateCode, tenantId);
    }

    public async Task<List<string>> ListAvailableTemplatesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var templateCodes = await db.ReportTemplates
            .IgnoreQueryFilters()
            .Where(t => (t.TenantId == tenantId || t.IsSystemDefault) && t.IsActive && !t.IsDeleted)
            .Select(t => t.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return templateCodes;
    }

    public async Task DeleteCustomTemplateAsync(string templateCode, string tenantId, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var existing = await db.ReportTemplates
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Code == templateCode && !t.IsSystemDefault, cancellationToken);

        if (existing != null)
        {
            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            var cacheKey = $"report_template:{tenantId}:{templateCode}";
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogInformation("Deleted custom template '{TemplateCode}' for tenant '{TenantId}', reverted to System Default", templateCode, tenantId);
        }
    }
}
