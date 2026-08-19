using Mapster;
using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Extensions;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Queries.Orders.ListOrders;

public class ListOrdersQueryHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public ListOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<OrderDto>> Handle(ListOrdersQuery query, CancellationToken cancellationToken)
    {
        var baseQuery = _unitOfWork.GetRepository<Order>().Query();

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<OrderStatus>(query.Status, true, out var statusEnum))
        {
            baseQuery = baseQuery.Where(o => o.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            baseQuery = baseQuery.Where(o => o.CustomerName.ToLower().Contains(search) ||
                                             o.CustomerEmail.ToLower().Contains(search) ||
                                             o.OrderNumber.ToLower().Contains(search));
        }

        // Tự động: CountAsync + Skip + Take + ProjectToType<OrderDto> + ToListAsync chỉ bằng 1 dòng gọi duy nhất!
        return await baseQuery
            .OrderByDescending(o => o.CreatedAt)
            .ProjectToType<OrderDto>()
            .ToPagedResultAsync(query.PageIndex, query.PageSize, cancellationToken);
    }
}
