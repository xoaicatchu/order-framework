namespace WolverineApp.Application.Common.Interfaces;

public interface ICacheService
{
    ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    ValueTask SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    ValueTask<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);

    ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
}
