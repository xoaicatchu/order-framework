using FluentValidation;

namespace WolverineApp.Application.Commands.Orders.CancelOrder;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}
