using Shouldly;
using WolverineApp.Application.Commands.Orders.CreateOrder;
using Xunit;

namespace Order.Application.UnitTests;

public class ValidationTests
{
    [Fact]
    public async Task CreateOrderValidatorRejectsEmptyItems()
    {
        var result = await new CreateOrderCommandValidator().ValidateAsync(
            new CreateOrderCommand("Customer", "customer@example.com", []));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == "Items");
    }
}
