namespace WolverineApp.Application.Common.Interfaces;

public interface IOutboxSignal
{
    void Signal();
    Task WaitAsync(CancellationToken cancellationToken);
}
