using WolverineApp.Domain.Common;

namespace WolverineApp.Infrastructure.Persistence.Models;

public class OutboxRecord : IMultiTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MessageType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOnUtc { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextAttemptAtUtc { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
    public string? LockOwner { get; set; }
    public string? Error { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}
