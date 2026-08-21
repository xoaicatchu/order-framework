using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Infrastructure.Persistence.Models;

namespace WolverineApp.Infrastructure.Messaging;

public class IdempotencyService : IIdempotencyService
{
    private readonly IUnitOfWork _unitOfWork;

    public IdempotencyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HasBeenProcessedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.GetRepository<ProcessedMessageRecord>().Query()
            .AnyAsync(p => p.MessageId == messageId && p.ConsumerName == consumerName, cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.GetRepository<ProcessedMessageRecord>().AddAsync(new ProcessedMessageRecord
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            ConsumerName = consumerName,
            ProcessedOnUtc = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
