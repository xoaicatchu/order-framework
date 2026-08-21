using Shouldly;
using WolverineApp.Domain.Orders;
using Xunit;
using DomainOrder = WolverineApp.Domain.Orders.Order;

namespace Order.Domain.UnitTests;

public class OrderStatusTests
{
    [Fact]
    public void NewOrderStartsPending()
    {
        var order = DomainOrder.Create(
            "Customer",
            "customer@example.com",
            "tenant-a",
            [new OrderItem { ProductName = "Product", Quantity = 2, UnitPrice = 10m }]);

        order.Status.ShouldBe(OrderStatus.Pending);
        order.TotalAmount.ShouldBe(20m);
    }

    [Fact]
    public void OrderCanFollowTheHappyPath()
    {
        var order = DomainOrder.Create(
            "Customer",
            "customer@example.com",
            "tenant-a",
            [new OrderItem { ProductName = "Product", Quantity = 1, UnitPrice = 10m }]);

        order.Confirm();
        order.StartProcessing();
        order.Ship();
        order.Deliver();

        order.Status.ShouldBe(OrderStatus.Delivered);
    }
}
