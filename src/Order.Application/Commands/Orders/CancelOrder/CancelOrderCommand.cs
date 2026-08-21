using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;

namespace WolverineApp.Application.Commands.Orders.CancelOrder;

public record CancelOrderCommand(
    Guid OrderId,
    bool IsConfirmed = false
) : ICommand<OrderDto>;
