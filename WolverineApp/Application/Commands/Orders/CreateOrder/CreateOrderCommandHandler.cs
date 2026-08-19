using Mapster;
using WolverineApp.Application.Common.Caching;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Commands.Orders.CreateOrder;

public class CreateOrderCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ITenantProvider _tenantProvider;

    public CreateOrderCommandHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _tenantProvider = tenantProvider;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // Inbound mapping: CreateOrderItemDto -> OrderItem via Mapster
        var items = command.Items.Adapt<List<OrderItem>>();
        var order = Order.Create(command.CustomerName, command.CustomerEmail, items);

        await _unitOfWork.GetRepository<Order>().AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache Invalidation: Xóa cache thống kê và danh sách đơn của Tenant
        var tenantId = _tenantProvider.TenantId;
        await _cacheService.RemoveAsync(CacheKeys.Statistics(tenantId), cancellationToken);
        await _cacheService.RemoveByTagAsync(CacheKeys.OrderTag(tenantId), cancellationToken);

        // Outbound mapping: Order -> OrderDto via Mapster
        return order.Adapt<OrderDto>();
    }
}
