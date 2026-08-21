using Microsoft.Extensions.Logging;
using Wolverine;
using WolverineApp.Application.Common.Caching;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Middleware;

public static class CacheInvalidationMiddleware
{
    public static async Task AfterAsync(
        Envelope envelope,
        ICacheService cacheService,
        ITenantProvider tenantProvider,
        ILogger<ICacheService> logger,
        CancellationToken cancellationToken)
    {
        if (envelope.Message is IBaseCommand command)
        {
            var tenantId = tenantProvider.TenantId;
            logger.LogDebug("Invalidating cache for Tenant: {TenantId} after executing {CommandType}",
                tenantId, command.GetType().Name);

            await cacheService.RemoveByTagAsync(CacheKeys.OrderTag(tenantId), cancellationToken);
            await cacheService.RemoveAsync(CacheKeys.Statistics(tenantId), cancellationToken);
        }
    }
}
