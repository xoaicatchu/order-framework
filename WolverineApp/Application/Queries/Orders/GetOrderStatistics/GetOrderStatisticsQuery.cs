using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;

namespace WolverineApp.Application.Queries.Orders.GetOrderStatistics;

public record GetOrderStatisticsQuery : IQuery<OrderStatisticsDto>;
