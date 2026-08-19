using WolverineApp.Domain.Common;
using WolverineApp.Domain.Events;

namespace WolverineApp.Domain.Orders;

public class Order : BaseAuditableEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public List<OrderItem> Items { get; set; } = [];

    public static Order Create(string customerName, string customerEmail, IEnumerable<OrderItem> items)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            Status = OrderStatus.Pending
        };

        foreach (var item in items)
        {
            item.OrderId = order.Id;
            if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
            if (item.Total == 0) item.Total = item.Quantity * item.UnitPrice;
            order.Items.Add(item);
        }

        order.TotalAmount = order.Items.Sum(i => i.Total);

        // Tự động phát sinh Domain Event khi tạo đơn hàng
        order.AddDomainEvent(new OrderCreatedDomainEvent(
            order.Id,
            order.OrderNumber,
            order.CustomerName,
            order.CustomerEmail,
            order.TotalAmount,
            order.TenantId));

        return order;
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (Status == newStatus) return;

        var oldStatus = Status;
        Status = newStatus;

        // Tự động phát sinh Domain Event khi thay đổi trạng thái
        AddDomainEvent(new OrderStatusChangedDomainEvent(
            Id,
            OrderNumber,
            oldStatus,
            newStatus,
            TenantId));
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled.");

        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new InvalidOperationException($"Cannot cancel order in '{Status}' status.");

        Status = OrderStatus.Cancelled;

        // Tự động phát sinh Domain Event khi hủy đơn
        AddDomainEvent(new OrderCancelledDomainEvent(
            Id,
            OrderNumber,
            TotalAmount,
            CustomerEmail,
            TenantId));
    }
}
