using WolverineApp.Domain.Common;

namespace WolverineApp.Domain.Reporting;

public class ReportTemplate : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "General";
    public string Content { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public bool IsSystemDefault { get; set; }

    public static ReportTemplate Create(
        string code,
        string name,
        string content,
        string tenantId,
        string? description = null,
        string category = "General",
        bool isSystemDefault = false)
    {
        return new ReportTemplate
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Content = content,
            TenantId = tenantId,
            Description = description,
            Category = category,
            IsActive = true,
            IsSystemDefault = isSystemDefault,
            Version = 1
        };
    }

    public void UpdateContent(string content, string updatedBy)
    {
        Content = content;
        Version++;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}
