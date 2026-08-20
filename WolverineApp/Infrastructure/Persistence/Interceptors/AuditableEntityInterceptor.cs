using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Audit;

namespace WolverineApp.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public AuditableEntityInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var entries = eventData.Context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        if (!entries.Any())
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditLogs = new List<AuditLog>();
        var currentTenant = _tenantProvider.TenantId;

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;
            var action = entry.State.ToString();
            var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? string.Empty;

            var propertyValues = entry.Properties
                .Where(p => !p.Metadata.IsPrimaryKey())
                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

            var details = JsonSerializer.Serialize(propertyValues);

            auditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                TenantId = currentTenant,
                Action = action,
                EntityName = entityName,
                EntityId = primaryKey,
                Details = details.Length > 1900 ? details[..1900] : details,
                Timestamp = DateTime.UtcNow,
                IsSuccess = true
            });
        }

        eventData.Context.Set<AuditLog>().AddRange(auditLogs);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
