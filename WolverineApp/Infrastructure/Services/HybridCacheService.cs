using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Services;

#pragma warning disable EXTEXP0018
public class HybridCacheService : ICacheService
{
    private readonly HybridCache _cache;
    private readonly ILogger<HybridCacheService> _logger;

    public HybridCacheService(HybridCache cache, ILogger<HybridCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var options = expiration.HasValue
            ? new HybridCacheEntryOptions
            {
                Expiration = expiration.Value,
                LocalCacheExpiration = TimeSpan.FromSeconds(Math.Max(5, expiration.Value.TotalSeconds / 2))
            }
            : null;

        return await _cache.GetOrCreateAsync(
            key,
            async ct =>
            {
                _logger.LogDebug("🔄 [HybridCache] Cache miss for key: {Key}. Fetching from underlying source...", key);
                return await factory(ct);
            },
            options,
            tags,
            cancellationToken);
    }

    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("🗑️ [HybridCache] Evicting cache key: {Key}", key);
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public async ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("🏷️ [HybridCache] Evicting all keys with tag: {Tag}", tag);
        await _cache.RemoveByTagAsync(tag, cancellationToken);
    }
}
#pragma warning restore EXTEXP0018
