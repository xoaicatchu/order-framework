using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.DTOs.Orders;

namespace WolverineApp.Application.Queries.Orders.ListOrders;

public record ListOrdersQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? Status = null,
    string? Search = null
) : IQuery<PagedResult<OrderDto>>;
