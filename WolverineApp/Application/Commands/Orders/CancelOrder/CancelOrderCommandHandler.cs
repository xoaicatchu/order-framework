using Mapster;
using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Caching;
using WolverineApp.Application.Common.Exceptions;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Commands.Orders.CancelOrder;

public class CancelOrderCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ITenantProvider _tenantProvider;

    public CancelOrderCommandHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _tenantProvider = tenantProvider;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.GetRepository<Order>()
            .Query(tracking: true)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException($"Không tìm thấy đơn hàng với mã: {command.OrderId}");

        if (!command.IsConfirmed)
        {
            throw new BusinessConfirmationException(
                $"Bạn có chắc chắn muốn hủy đơn hàng #{order.OrderNumber} (Tổng tiền: ${order.TotalAmount:N2}) không?",
                new { order.Id, order.OrderNumber, order.TotalAmount }
            );
        }

        order.Cancel();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache Invalidation: Xóa cache chi tiết đơn và thống kê của Tenant
        var tenantId = _tenantProvider.TenantId;
        await _cacheService.RemoveAsync(CacheKeys.Order(tenantId, command.OrderId), cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.Statistics(tenantId), cancellationToken);
        await _cacheService.RemoveByTagAsync(CacheKeys.OrderTag(tenantId), cancellationToken);

        return order.Adapt<OrderDto>();
    }
}
