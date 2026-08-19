using Microsoft.Extensions.Logging;
using WolverineApp.Domain.Events;

namespace WolverineApp.Application.Events;

public class SendEmailOnOrderCreatedHandler
{
    private readonly ILogger<SendEmailOnOrderCreatedHandler> _logger;

    public SendEmailOnOrderCreatedHandler(ILogger<SendEmailOnOrderCreatedHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Giả lập gửi email xác nhận đặt hàng không đồng bộ (Zero latency cho Client API)
        await Task.Delay(10, cancellationToken);

        _logger.LogInformation(
            "📧 [Email Service] Confirmation email sent to {CustomerEmail} for Order #{OrderNumber} (Total: ${TotalAmount:F2}) | Tenant: {TenantId}",
            domainEvent.CustomerEmail,
            domainEvent.OrderNumber,
            domainEvent.TotalAmount,
            domainEvent.TenantId);
    }
}
