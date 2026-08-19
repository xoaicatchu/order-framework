using Mapster;
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
        // Inbound mapping: CreateOrderItemDto -> OrderItem via Mapster
        var items = command.Items.Adapt<List<OrderItem>>();
        var order = Order.Create(command.CustomerName, command.CustomerEmail, items);

        await _unitOfWork.GetRepository<Order>().AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Outbound mapping: Order -> OrderDto via Mapster
        return order.Adapt<OrderDto>();
    }
}
