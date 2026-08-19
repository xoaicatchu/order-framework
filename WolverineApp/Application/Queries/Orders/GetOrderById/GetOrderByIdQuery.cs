using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;

namespace WolverineApp.Application.Queries.Orders.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto>;
