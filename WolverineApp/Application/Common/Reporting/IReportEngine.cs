namespace WolverineApp.Application.Common.Reporting;

public interface IReportEngine
{
    Task<ReportRenderResult> RenderAsync(ReportRenderRequest request, CancellationToken cancellationToken = default);
    Task<string> RenderHtmlAsync(string templateCode, object dataModel, string? tenantId = null, CancellationToken cancellationToken = default);
}
