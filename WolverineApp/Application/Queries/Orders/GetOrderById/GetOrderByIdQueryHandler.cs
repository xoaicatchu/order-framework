using Mapster;
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
        var orderDto = await _unitOfWork.GetRepository<Order>()
            .Query()
            .Where(o => o.Id == query.Id)
            .ProjectToType<OrderDto>()
            .FirstOrDefaultAsync(cancellationToken);

        if (orderDto is null)
            throw new KeyNotFoundException($"Order not found: {query.Id}");

        return orderDto;
    }
}
