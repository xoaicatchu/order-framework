using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Queries.Orders.GetOrderById;

public class GetOrderByIdQueryHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.GetRepository<Order>()
            .Query() // Default AsNoTracking!
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == query.Id, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException($"Order not found: {query.Id}");

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
