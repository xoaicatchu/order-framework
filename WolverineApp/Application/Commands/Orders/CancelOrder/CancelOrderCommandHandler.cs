using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Exceptions;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Commands.Orders.CancelOrder;

public class CancelOrderCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

        return new OrderDto(
            order.Id,
            order.OrderNumber,
            order.CustomerName,
            order.CustomerEmail,
            order.TotalAmount,
            order.Status.ToString(),
            order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(i.Id, i.ProductName, i.Sku, i.Quantity, i.UnitPrice, i.Total)).ToList()
        );
    }
}
