namespace WolverineApp.Application.DTOs.Orders;

public record OrderDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    Guid Id,
    string ProductName,
    string? Sku,
    int Quantity,
    decimal UnitPrice,
    decimal Total
);
