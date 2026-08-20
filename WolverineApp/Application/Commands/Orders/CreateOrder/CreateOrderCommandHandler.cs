using Mapster;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Commands.Orders.CreateOrder;

public class CreateOrderCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var items = command.Items.Adapt<List<OrderItem>>();
        var order = Order.Create(command.CustomerName, command.CustomerEmail, _tenantProvider.TenantId, items);

        await _unitOfWork.GetRepository<Order>().AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Adapt<OrderDto>();
    }
}
