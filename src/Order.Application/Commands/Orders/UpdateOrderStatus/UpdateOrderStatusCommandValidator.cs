using FluentValidation;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Commands.Orders.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => Enum.TryParse<OrderStatus>(status, true, out _))
            .WithMessage("Status must be a valid order status (Pending, Confirmed, Processing, Shipped, Delivered, Cancelled).");
    }
}
