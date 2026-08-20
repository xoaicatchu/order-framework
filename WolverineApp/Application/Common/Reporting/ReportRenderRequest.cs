namespace WolverineApp.Application.Common.Reporting;

public record ReportRenderRequest(
    string TemplateCode,
    object DataModel,
    ReportOutputFormat Format = ReportOutputFormat.Pdf,
    Dictionary<string, object>? Parameters = null
);
