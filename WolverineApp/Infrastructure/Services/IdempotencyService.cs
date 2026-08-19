using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Infrastructure.Data;
using WolverineApp.Infrastructure.Data.Entities;

namespace WolverineApp.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly ApplicationDbContext _dbContext;

    public IdempotencyService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasBeenProcessedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProcessedMessages
            .AnyAsync(p => p.MessageId == messageId && p.ConsumerName == consumerName, cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default)
    {
        _dbContext.ProcessedMessages.Add(new ProcessedMessage
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            ConsumerName = consumerName,
            ProcessedOnUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
