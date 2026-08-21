using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;

namespace WolverineApp.Application.Commands.Orders.CreateOrder;

public record CreateOrderCommand(
    string CustomerName,
    string CustomerEmail,
    List<CreateOrderItemDto> Items
) : ICommand<OrderDto>;
