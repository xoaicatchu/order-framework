using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Events;

namespace WolverineApp.Application.Events;

public class ReserveInventoryOnOrderCreatedHandler
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<ReserveInventoryOnOrderCreatedHandler> _logger;

    public ReserveInventoryOnOrderCreatedHandler(
        IIdempotencyService idempotencyService,
        ILogger<ReserveInventoryOnOrderCreatedHandler> logger)
    {
        _idempotencyService = idempotencyService;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        const string consumerName = nameof(ReserveInventoryOnOrderCreatedHandler);

        // Kiểm tra chống xử lý lặp (Idempotent Consumer)
        if (await _idempotencyService.HasBeenProcessedAsync(domainEvent.EventId, consumerName, cancellationToken))
        {
            _logger.LogWarning(
                "⏭️ [Inventory Service] Event {EventId} for Order #{OrderNumber} already processed. Skipping duplicate.",
                domainEvent.EventId,
                domainEvent.OrderNumber);
            return;
        }

        // Giả lập giữ hàng trong kho
        await Task.Delay(10, cancellationToken);

        _logger.LogInformation(
            "📦 [Inventory Service] Inventory reserved successfully for Order #{OrderNumber} | Amount: ${TotalAmount:F2} | Tenant: {TenantId}",
            domainEvent.OrderNumber,
            domainEvent.TotalAmount,
            domainEvent.TenantId);

        // Đánh dấu đã xử lý thành công
        await _idempotencyService.MarkAsProcessedAsync(domainEvent.EventId, consumerName, cancellationToken);
    }
}
