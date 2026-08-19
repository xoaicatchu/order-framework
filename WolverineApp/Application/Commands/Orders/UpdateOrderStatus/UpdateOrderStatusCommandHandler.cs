using Mapster;
using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Caching;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Commands.Orders.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ITenantProvider _tenantProvider;

    public UpdateOrderStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _tenantProvider = tenantProvider;
    }

    public async Task<OrderDto> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.GetRepository<Order>()
            .Query(tracking: true)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException($"Order not found: {command.OrderId}");

        var newStatus = Enum.Parse<OrderStatus>(command.Status, true);
        order.UpdateStatus(newStatus);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache Invalidation: Xóa cache chi tiết đơn và thống kê của Tenant
        var tenantId = _tenantProvider.TenantId;
        await _cacheService.RemoveAsync(CacheKeys.Order(tenantId, command.OrderId), cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.Statistics(tenantId), cancellationToken);
        await _cacheService.RemoveByTagAsync(CacheKeys.OrderTag(tenantId), cancellationToken);

        return order.Adapt<OrderDto>();
    }
}
