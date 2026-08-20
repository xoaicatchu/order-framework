namespace WolverineApp.Application.Common.Reporting;

public record ReportRenderResult(
    byte[] Content,
    string ContentType,
    string FileName
);
