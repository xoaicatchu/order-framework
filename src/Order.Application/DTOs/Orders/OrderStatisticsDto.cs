namespace WolverineApp.Application.DTOs.Orders;

public record OrderStatisticsDto(
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    Dictionary<string, int> OrdersByStatus
);
