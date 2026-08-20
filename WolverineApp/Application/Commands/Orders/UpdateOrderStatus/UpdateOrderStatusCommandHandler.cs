using Mapster;
using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Commands.Orders.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
        switch (newStatus)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Processing:
                order.StartProcessing();
                break;
            case OrderStatus.Shipped:
                order.Ship();
                break;
            case OrderStatus.Delivered:
                order.Deliver();
                break;
            case OrderStatus.Pending:
                throw new InvalidOperationException("An order cannot be moved back to Pending.");
            case OrderStatus.Cancelled:
                throw new InvalidOperationException("Use the cancel endpoint and Orders:Cancel permission to cancel an order.");
            default:
                throw new InvalidOperationException($"Unsupported order status '{command.Status}'.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Adapt<OrderDto>();
    }
}
