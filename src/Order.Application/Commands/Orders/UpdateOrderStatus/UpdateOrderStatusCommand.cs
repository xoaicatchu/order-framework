using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Orders;

namespace WolverineApp.Application.Commands.Orders.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, string Status) : ICommand<OrderDto>;
