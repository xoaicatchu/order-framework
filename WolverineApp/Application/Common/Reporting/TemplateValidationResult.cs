namespace WolverineApp.Application.Common.Reporting;

public record TemplateValidationResult(
    bool IsValid,
    string? ErrorMessage = null,
    int? Line = null,
    int? Column = null
)
{
    public static TemplateValidationResult Success() => new(true);
    public static TemplateValidationResult Error(string message, int? line = null, int? column = null) => new(false, message, line, column);
}

public record ValidateTemplateRequest(string TemplateContent);
