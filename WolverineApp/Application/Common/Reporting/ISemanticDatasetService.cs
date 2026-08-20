using WolverineApp.Domain.Reporting;

namespace WolverineApp.Application.Common.Reporting;

public record SemanticDatasetDto(
    string Code,
    string Name,
    string Category,
    string? Description,
    List<SemanticFieldDefinition> Fields
);

public interface ISemanticDatasetService
{
    Task<List<SemanticDatasetDto>> GetAvailableDatasetsAsync(CancellationToken cancellationToken = default);
    Task<SemanticDatasetDto?> GetDatasetByCodeAsync(string datasetCode, CancellationToken cancellationToken = default);
    Task<List<Dictionary<string, object?>>> ExecuteDatasetQueryAsync(
        string datasetCode,
        string tenantId,
        List<string> selectedFields,
        Dictionary<string, object?> filterCriteria,
        CancellationToken cancellationToken = default);
    string GenerateDefaultLiquidTemplate(string reportName, List<SemanticFieldDefinition> selectedFields);
}
