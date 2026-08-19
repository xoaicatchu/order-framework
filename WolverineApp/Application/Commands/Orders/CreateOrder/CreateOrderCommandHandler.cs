using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Commands.Orders.CreateOrder;

public class CreateOrderCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = Order.Create(
            command.CustomerName,
            command.CustomerEmail,
            command.Items.Select(i => (i.ProductName, i.Sku, i.Quantity, i.UnitPrice))
        );

        await _unitOfWork.GetRepository<Order>().AddAsync(order, cancellationToken);
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
