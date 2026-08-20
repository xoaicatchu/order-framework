using WolverineApp.Domain.Common;

namespace WolverineApp.Domain.Reporting;

public class ReportFilterConfigItem
{
    public string FieldName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FilterType { get; set; } = "date_range"; // date_range, date, select, multi_select, text, number, boolean
    public bool Required { get; set; } = false;
    public string? DefaultValue { get; set; }
    public string? DataSourceUrl { get; set; }
}

public class ReportConfiguration : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DatasetCode { get; set; } = string.Empty;
    public string SelectedFieldsJson { get; set; } = "[]";
    public string FilterConfigJson { get; set; } = "[]";
    public string TemplateContent { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public static ReportConfiguration Create(
        string code,
        string name,
        string datasetCode,
        string tenantId,
        string selectedFieldsJson,
        string filterConfigJson,
        string templateContent)
    {
        return new ReportConfiguration
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            DatasetCode = datasetCode,
            TenantId = tenantId,
            SelectedFieldsJson = selectedFieldsJson,
            FilterConfigJson = filterConfigJson,
            TemplateContent = templateContent,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string selectedFieldsJson,
        string filterConfigJson,
        string templateContent,
        string updatedBy)
    {
        Name = name;
        SelectedFieldsJson = selectedFieldsJson;
        FilterConfigJson = filterConfigJson;
        TemplateContent = templateContent;
        Version++;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}
