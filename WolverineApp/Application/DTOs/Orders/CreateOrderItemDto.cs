namespace WolverineApp.Application.DTOs.Orders;

public record CreateOrderItemDto(
    string ProductName,
    string? Sku,
    int Quantity,
    decimal UnitPrice
);
