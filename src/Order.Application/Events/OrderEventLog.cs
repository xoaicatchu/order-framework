using Microsoft.Extensions.Logging;

namespace WolverineApp.Application.Events;

internal static partial class OrderEventLog
{
    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Confirmation email sent to {CustomerEmail} for order {OrderNumber} (Total: {TotalAmount}) | Tenant: {TenantId}")]
    public static partial void ConfirmationEmailSent(ILogger logger, string customerEmail, string orderNumber, decimal totalAmount, string tenantId);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning, Message = "Inventory reservation event {EventId} for order {OrderNumber} was already processed.")]
    public static partial void InventoryReservationAlreadyProcessed(ILogger logger, Guid eventId, string orderNumber);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "Inventory reserved for order {OrderNumber} | Amount: {TotalAmount} | Tenant: {TenantId}")]
    public static partial void InventoryReserved(ILogger logger, string orderNumber, decimal totalAmount, string tenantId);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Warning, Message = "Inventory release event {EventId} for order {OrderNumber} was already processed.")]
    public static partial void InventoryReleaseAlreadyProcessed(ILogger logger, Guid eventId, string orderNumber);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Information, Message = "Inventory released for cancelled order {OrderNumber} | Refund amount: {TotalAmount} | Customer: {CustomerEmail}")]
    public static partial void InventoryReleased(ILogger logger, string orderNumber, decimal totalAmount, string customerEmail);
}
