using WolverineApp.Domain.Common;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Domain.Events;

public record OrderStatusChangedDomainEvent(
    Guid OrderId,
    string OrderNumber,
    OrderStatus OldStatus,
    OrderStatus NewStatus,
    string TenantId,
    Guid EventId = default,
    DateTime OccurredOnUtc = default
) : IDomainEvent
{
    public Guid EventId { get; init; } = EventId == default ? Guid.NewGuid() : EventId;
    public DateTime OccurredOnUtc { get; init; } = OccurredOnUtc == default ? DateTime.UtcNow : OccurredOnUtc;
}
