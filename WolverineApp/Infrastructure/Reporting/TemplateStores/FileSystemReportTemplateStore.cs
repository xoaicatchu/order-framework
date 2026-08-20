using Microsoft.Extensions.Hosting;
using WolverineApp.Application.Common.Reporting;

namespace WolverineApp.Infrastructure.Reporting.TemplateStores;

public class FileSystemReportTemplateStore : IReportTemplateStore
{
    private readonly string _baseTemplatesPath;

    public FileSystemReportTemplateStore(IHostEnvironment hostEnvironment)
    {
        _baseTemplatesPath = Path.Combine(hostEnvironment.ContentRootPath, "Infrastructure", "Reporting", "Templates");
        if (!Directory.Exists(_baseTemplatesPath))
        {
            Directory.CreateDirectory(_baseTemplatesPath);
        }
    }

    public async Task<string?> GetTemplateContentAsync(string templateCode, string tenantId, CancellationToken cancellationToken = default)
    {
        // 1. Kiểm tra template riêng của Tenant: Templates/{tenantId}/{templateCode}.liquid
        var tenantSpecificFile = Path.Combine(_baseTemplatesPath, tenantId, $"{templateCode}.liquid");
        if (File.Exists(tenantSpecificFile))
        {
            return await File.ReadAllTextAsync(tenantSpecificFile, cancellationToken);
        }

        // 2. Fallback về template chung của hệ thống: Templates/{templateCode}.liquid
        var defaultFile = Path.Combine(_baseTemplatesPath, $"{templateCode}.liquid");
        if (File.Exists(defaultFile))
        {
            return await File.ReadAllTextAsync(defaultFile, cancellationToken);
        }

        return null;
    }

    public async Task SaveCustomTemplateAsync(string templateCode, string tenantId, string content, CancellationToken cancellationToken = default)
    {
        var tenantDir = Path.Combine(_baseTemplatesPath, tenantId);
        if (!Directory.Exists(tenantDir))
        {
            Directory.CreateDirectory(tenantDir);
        }

        var tenantSpecificFile = Path.Combine(tenantDir, $"{templateCode}.liquid");
        await File.WriteAllTextAsync(tenantSpecificFile, content, cancellationToken);
    }

    public Task<List<string>> ListAvailableTemplatesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var templates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Lấy tất cả template chung
        if (Directory.Exists(_baseTemplatesPath))
        {
            foreach (var file in Directory.GetFiles(_baseTemplatesPath, "*.liquid", SearchOption.TopDirectoryOnly))
            {
                templates.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        // Lấy tất cả template của Tenant
        var tenantDir = Path.Combine(_baseTemplatesPath, tenantId);
        if (Directory.Exists(tenantDir))
        {
            foreach (var file in Directory.GetFiles(tenantDir, "*.liquid", SearchOption.TopDirectoryOnly))
            {
                templates.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        return Task.FromResult(templates.ToList());
    }
}
