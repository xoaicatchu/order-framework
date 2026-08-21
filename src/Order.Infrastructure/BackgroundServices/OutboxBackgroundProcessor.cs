using System.Data;
using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Infrastructure.Options;
using WolverineApp.Infrastructure.Persistence.Models;

namespace WolverineApp.Infrastructure.BackgroundServices;

public class OutboxBackgroundProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxBackgroundProcessor> _logger;
    private readonly IOutboxSignal _outboxSignal;
    private readonly OutboxOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OutboxBackgroundProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxBackgroundProcessor> logger,
        IOutboxSignal outboxSignal,
        IOptions<OutboxOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _outboxSignal = outboxSignal;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started with signal wake-up and database lease.");

        try
        {
            await ProcessAvailableBatchesAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initial outbox scan failed; fallback loop will retry.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                try
                {
                    await _outboxSignal.WaitAsync(stoppingToken)
                        .WaitAsync(_options.FallbackScanInterval, stoppingToken);
                }
                catch (TimeoutException)
                {
                    // Fallback scan prevents lost signals from leaving messages pending.
                }

                await ProcessAvailableBatchesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox batch failed.");
            }
        }
    }

    private async Task ProcessAvailableBatchesAsync(CancellationToken cancellationToken)
    {
        while (await ProcessOutboxBatchAsync(cancellationToken))
        {
        }
    }

    private async Task<bool> ProcessOutboxBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var lockOwner = $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";
        var now = DateTime.UtcNow;

        var candidates = await unitOfWork.GetRepository<OutboxRecord>().Query()
            .Where(m => m.ProcessedOnUtc == null
                        && m.RetryCount < _options.MaxRetries
                        && (m.NextAttemptAtUtc == null || m.NextAttemptAtUtc <= now)
                        && (m.LockedUntilUtc == null || m.LockedUntilUtc <= now))
            .OrderBy(m => m.OccurredOnUtc)
            .Take(Math.Clamp(_options.BatchSize, 1, 500))
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return false;
        }

        var claimedIds = new List<Guid>(candidates.Count);
        foreach (var candidate in candidates)
        {
            if (await TryClaimAsync(
                    unitOfWork.GetDbConnection(),
                    candidate.Id,
                    lockOwner,
                    now,
                    cancellationToken))
            {
                claimedIds.Add(candidate.Id);
            }
        }

        if (claimedIds.Count == 0)
        {
            return false;
        }

        var messages = await unitOfWork.GetRepository<OutboxRecord>().Query(tracking: true)
            .Where(m => claimedIds.Contains(m.Id) && m.LockOwner == lockOwner)
            .OrderBy(m => m.OccurredOnUtc)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.MessageType);
                if (eventType == null)
                {
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    message.Error = $"Unknown event type: {message.MessageType}";
                    ClearLease(message);
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType, JsonOptions);
                if (domainEvent == null)
                {
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    message.Error = "Deserialization failed";
                    ClearLease(message);
                    continue;
                }

                await messageBus.PublishAsync(domainEvent);

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
                message.NextAttemptAtUtc = null;
                ClearLease(message);

                _logger.LogInformation(
                    "Dispatched outbox event {EventType} ({MessageId}) for tenant {TenantId}.",
                    eventType.Name,
                    message.Id,
                    message.TenantId);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                var retryDelaySeconds = Math.Min(Math.Pow(2, message.RetryCount) * 5, TimeSpan.FromHours(1).TotalSeconds);
                message.NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(retryDelaySeconds);
                ClearLease(message);

                _logger.LogWarning(
                    ex,
                    "Outbox event {MessageId} failed. Retry {RetryCount}/{MaxRetries}.",
                    message.Id,
                    message.RetryCount,
                    _options.MaxRetries);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ClearLease(OutboxRecord message)
    {
        message.LockOwner = null;
        message.LockedUntilUtc = null;
    }

    private async Task<bool> TryClaimAsync(
        DbConnection connection,
        Guid messageId,
        string lockOwner,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE "OutboxMessages"
            SET "LockOwner" = @lockOwner,
                "LockedUntilUtc" = @lockedUntilUtc
            WHERE "Id" = @id
              AND "ProcessedOnUtc" IS NULL
              AND "RetryCount" < @maxRetries
              AND ("NextAttemptAtUtc" IS NULL OR "NextAttemptAtUtc" <= @now)
              AND ("LockedUntilUtc" IS NULL OR "LockedUntilUtc" <= @now)
            """;

        AddParameter(command, "@lockOwner", lockOwner);
        AddParameter(command, "@lockedUntilUtc", now.Add(_options.LeaseDuration));
        AddParameter(command, "@id", messageId);
        AddParameter(command, "@maxRetries", _options.MaxRetries);
        AddParameter(command, "@now", now);

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
