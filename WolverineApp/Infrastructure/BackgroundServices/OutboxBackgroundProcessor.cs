using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wolverine;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Infrastructure.BackgroundServices;

public class OutboxBackgroundProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxBackgroundProcessor> _logger;
    private const int BatchSize = 20;
    private const int MaxRetries = 3;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OutboxBackgroundProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxBackgroundProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 [Outbox Processor] Background service started successfully.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [Outbox Processor] Error occurred while processing outbox batch: {Message}", ex.Message);
            }

            // Polling interval (chạy định kỳ mỗi 2 giây)
            await Task.Delay(2000, stoppingToken);
        }
    }

    private async Task ProcessOutboxBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        // Lấy danh sách các message chưa được xử lý hoặc đang chờ retry
        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.MessageType);
                if (eventType == null)
                {
                    _logger.LogError("❌ [Outbox Processor] Unknown event type: {MessageType} for message {Id}", message.MessageType, message.Id);
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    message.Error = $"Unknown event type: {message.MessageType}";
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType, JsonOptions);
                if (domainEvent == null)
                {
                    _logger.LogError("❌ [Outbox Processor] Deserialization failed for message {Id}", message.Id);
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    message.Error = "Deserialization failed";
                    continue;
                }

                // Dispatch sự kiện vào Wolverine Message Bus
                await messageBus.PublishAsync(domainEvent);

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;

                _logger.LogInformation(
                    "📤 [Outbox Processor] Dispatched event {EventType} (Id: {Id}) to Wolverine Bus | Tenant: {TenantId}",
                    eventType.Name,
                    message.Id,
                    message.TenantId);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;

                _logger.LogWarning(
                    ex,
                    "⚠️ [Outbox Processor] Failed to process message {Id}. Retry count: {RetryCount}/{MaxRetries}",
                    message.Id,
                    message.RetryCount,
                    MaxRetries);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
