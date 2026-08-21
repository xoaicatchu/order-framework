using WolverineApp.Domain.Common;

namespace WolverineApp.Domain.Orders;

public class OrderItem : BaseAuditableEntity
{
    public Guid OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }

    public Order? Order { get; set; }
}
