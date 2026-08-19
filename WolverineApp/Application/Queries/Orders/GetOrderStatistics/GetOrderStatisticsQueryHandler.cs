using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Caching;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Queries.Orders.GetOrderStatistics;

public class GetOrderStatisticsQueryHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ITenantProvider _tenantProvider;

    public GetOrderStatisticsQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _tenantProvider = tenantProvider;
    }

    public async Task<OrderStatisticsDto> Handle(GetOrderStatisticsQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        var cacheKey = CacheKeys.Statistics(tenantId);

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var orders = await _unitOfWork.GetRepository<Order>()
                    .Query()
                    .ToListAsync(ct);

                var totalOrders = orders.Count;
                var totalRevenue = orders.Sum(o => o.TotalAmount);
                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                var ordersByStatus = orders
                    .GroupBy(o => o.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                return new OrderStatisticsDto(
                    totalOrders,
                    totalRevenue,
                    averageOrderValue,
                    ordersByStatus
                );
            },
            expiration: TimeSpan.FromSeconds(30),
            tags: [CacheKeys.OrderTag(tenantId)],
            cancellationToken: cancellationToken);
    }
}
