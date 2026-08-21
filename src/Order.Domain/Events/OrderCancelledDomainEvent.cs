using WolverineApp.Domain.Common;

namespace WolverineApp.Domain.Events;

public record OrderCancelledDomainEvent(
    Guid OrderId,
    string OrderNumber,
    decimal TotalAmount,
    string CustomerEmail,
    string TenantId,
    Guid EventId = default,
    DateTime OccurredOnUtc = default
) : IDomainEvent
{
    public Guid EventId { get; init; } = EventId == default ? Guid.NewGuid() : EventId;
    public DateTime OccurredOnUtc { get; init; } = OccurredOnUtc == default ? DateTime.UtcNow : OccurredOnUtc;
}
