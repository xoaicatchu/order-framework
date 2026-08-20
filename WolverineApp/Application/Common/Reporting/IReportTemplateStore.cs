namespace WolverineApp.Application.Common.Reporting;

public interface IReportTemplateStore
{
    Task<string?> GetTemplateContentAsync(string templateCode, string tenantId, CancellationToken cancellationToken = default);
    Task SaveCustomTemplateAsync(string templateCode, string tenantId, string content, CancellationToken cancellationToken = default);
    Task<List<string>> ListAvailableTemplatesAsync(string tenantId, CancellationToken cancellationToken = default);
}
