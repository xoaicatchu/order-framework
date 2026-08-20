using WolverineApp.Domain.Common;

namespace WolverineApp.Domain.Reporting;

public class SemanticFieldDefinition
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "string"; // string, number, currency, date, enum, boolean
    public bool Filterable { get; set; } = true;
    public List<string>? EnumValues { get; set; }
}

public class SemanticDataset : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string? Description { get; set; }
    public string FieldsMetadataJson { get; set; } = "[]";
    public string BaseQuerySql { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public static SemanticDataset Create(
        string code,
        string name,
        string category,
        string description,
        string fieldsMetadataJson,
        string baseQuerySql)
    {
        return new SemanticDataset
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Category = category,
            Description = description,
            FieldsMetadataJson = fieldsMetadataJson,
            BaseQuerySql = baseQuerySql,
            IsActive = true,
            TenantId = "system",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }
}
