using Microsoft.EntityFrameworkCore;
using WolverineApp.Domain.Audit;
using WolverineApp.Domain.Orders;
using WolverineApp.Infrastructure.Data.Entities;

namespace WolverineApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<ProcessedMessage> ProcessedMessages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
