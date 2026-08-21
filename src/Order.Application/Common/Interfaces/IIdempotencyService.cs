namespace WolverineApp.Application.Common.Interfaces;

public interface IIdempotencyService
{
    Task<bool> HasBeenProcessedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default);
}
