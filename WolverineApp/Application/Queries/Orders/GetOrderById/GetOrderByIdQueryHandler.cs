using Mapster;
using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Caching;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Queries.Orders.GetOrderById;

public class GetOrderByIdQueryHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ITenantProvider _tenantProvider;

    public GetOrderByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _tenantProvider = tenantProvider;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        var cacheKey = CacheKeys.Order(tenantId, query.Id);

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var orderDto = await _unitOfWork.GetRepository<Order>()
                    .Query()
                    .Where(o => o.Id == query.Id)
                    .ProjectToType<OrderDto>()
                    .FirstOrDefaultAsync(ct);

                if (orderDto is null)
                    throw new KeyNotFoundException($"Order not found: {query.Id}");

                return orderDto;
            },
            expiration: TimeSpan.FromMinutes(10),
            tags: [CacheKeys.OrderTag(tenantId)],
            cancellationToken: cancellationToken);
    }
}
