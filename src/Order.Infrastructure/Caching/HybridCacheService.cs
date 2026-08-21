using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Caching;

#pragma warning disable EXTEXP0018
public class HybridCacheService : ICacheService
{
    private readonly HybridCache _cache;
    private readonly ILogger<HybridCacheService> _logger;
    private readonly TimeSpan _localCacheMaximumLifetime;

    public HybridCacheService(
        HybridCache cache,
        ILogger<HybridCacheService> logger,
        IConfiguration configuration)
    {
        _cache = cache;
        _logger = logger;
        var seconds = configuration.GetValue("Cache:LocalCacheExpirationSeconds", 30);
        _localCacheMaximumLifetime = TimeSpan.FromSeconds(Math.Clamp(seconds, 1, 300));
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
                LocalCacheExpiration = GetLocalExpiration(expiration.Value)
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

    public async ValueTask SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var options = expiration.HasValue
            ? new HybridCacheEntryOptions
            {
                Expiration = expiration.Value,
                LocalCacheExpiration = GetLocalExpiration(expiration.Value)
            }
            : null;

        await _cache.SetAsync(key, value, options, tags, cancellationToken);
    }

    public async ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<T?>(
            key,
            _ => ValueTask.FromResult<T?>(default),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMilliseconds(1),
                LocalCacheExpiration = TimeSpan.FromMilliseconds(1)
            },
            cancellationToken: cancellationToken);
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

    private TimeSpan GetLocalExpiration(TimeSpan distributedExpiration)
        => distributedExpiration < _localCacheMaximumLifetime
            ? distributedExpiration
            : _localCacheMaximumLifetime;
}
#pragma warning restore EXTEXP0018
