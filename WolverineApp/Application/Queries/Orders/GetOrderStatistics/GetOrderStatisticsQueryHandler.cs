using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Queries.Orders.GetOrderStatistics;

public class GetOrderStatisticsQueryHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrderStatisticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderStatisticsDto> Handle(GetOrderStatisticsQuery query, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.GetRepository<Order>()
            .Query() // Default AsNoTracking!
            .ToListAsync(cancellationToken);

        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var ordersByStatus = orders
            .GroupBy(o => o.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return new OrderStatisticsDto(
            totalOrders,
            totalRevenue,
            averageOrderValue,
            ordersByStatus
        );
    }
}
