using WolverineApp.Domain.Common;

namespace WolverineApp.Domain.Audit;

public class AuditLog : IMultiTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public long DurationMs { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
}
