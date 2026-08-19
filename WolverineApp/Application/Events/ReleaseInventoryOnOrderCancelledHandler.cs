using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Events;

namespace WolverineApp.Application.Events;

public class ReleaseInventoryOnOrderCancelledHandler
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<ReleaseInventoryOnOrderCancelledHandler> _logger;

    public ReleaseInventoryOnOrderCancelledHandler(
        IIdempotencyService idempotencyService,
        ILogger<ReleaseInventoryOnOrderCancelledHandler> logger)
    {
        _idempotencyService = idempotencyService;
        _logger = logger;
    }

    public async Task Handle(OrderCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        const string consumerName = nameof(ReleaseInventoryOnOrderCancelledHandler);

        if (await _idempotencyService.HasBeenProcessedAsync(domainEvent.EventId, consumerName, cancellationToken))
        {
            _logger.LogWarning(
                "⏭️ [Inventory Service] Cancel event {EventId} for Order #{OrderNumber} already processed. Skipping duplicate.",
                domainEvent.EventId,
                domainEvent.OrderNumber);
            return;
        }

        // Giả lập hoàn kho
        await Task.Delay(10, cancellationToken);

        _logger.LogInformation(
            "🔄 [Inventory Service] Inventory released for cancelled Order #{OrderNumber} | Refund Amount: ${TotalAmount:F2} | Customer: {CustomerEmail}",
            domainEvent.OrderNumber,
            domainEvent.TotalAmount,
            domainEvent.CustomerEmail);

        await _idempotencyService.MarkAsProcessedAsync(domainEvent.EventId, consumerName, cancellationToken);
    }
}
