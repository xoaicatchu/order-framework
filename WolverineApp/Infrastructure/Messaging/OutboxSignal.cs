using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Messaging;

public sealed class OutboxSignal : IOutboxSignal, IDisposable
{
    private readonly SemaphoreSlim _signal = new(0, 1);

    public void Signal()
    {
        if (_signal.CurrentCount == 0)
        {
            _signal.Release();
        }
    }

    public Task WaitAsync(CancellationToken cancellationToken) => _signal.WaitAsync(cancellationToken);

    public void Dispose() => _signal.Dispose();
}
