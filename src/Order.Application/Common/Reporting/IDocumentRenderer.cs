namespace WolverineApp.Application.Common.Reporting;

public interface IDocumentRenderer
{
    ReportOutputFormat SupportedFormat { get; }
    Task<byte[]> RenderAsync(string templateCode, string compiledHtml, object dataModel, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);
}
