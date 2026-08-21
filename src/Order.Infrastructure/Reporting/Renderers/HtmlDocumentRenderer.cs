using System.Text;
using WolverineApp.Application.Common.Reporting;

namespace WolverineApp.Infrastructure.Reporting.Renderers;

public class HtmlDocumentRenderer : IDocumentRenderer
{
    public ReportOutputFormat SupportedFormat => ReportOutputFormat.Html;

    public Task<byte[]> RenderAsync(
        string templateCode,
        string compiledHtml,
        object dataModel,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.UTF8.GetBytes(compiledHtml);
        return Task.FromResult(bytes);
    }
}
