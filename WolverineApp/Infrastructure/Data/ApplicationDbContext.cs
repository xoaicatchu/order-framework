using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Audit;
using WolverineApp.Domain.Common;
using WolverineApp.Domain.Identity;
using WolverineApp.Domain.Orders;
using WolverineApp.Infrastructure.Data.Entities;

namespace WolverineApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserProvider _currentUserProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider tenantProvider,
        ICurrentUserProvider currentUserProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _currentUserProvider = currentUserProvider;
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    // Dynamic RBAC Entities
    public DbSet<AppRole> Roles => Set<AppRole>();
    public DbSet<AppRolePermission> RolePermissions => Set<AppRolePermission>();
    public DbSet<AppUserRole> UserRoles => Set<AppUserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Query Filters for Soft Delete & Multi-Tenancy
        modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        
        // Multi-Tenancy on Dynamic Roles (System Roles có thể truy cập được hoặc lọc theo Tenant)
        modelBuilder.Entity<AppRole>().HasQueryFilter(e => !e.IsDeleted && (e.TenantId == _tenantProvider.TenantId || e.IsSystemRole));
        modelBuilder.Entity<AppRolePermission>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId || e.Role.IsSystemRole);
        modelBuilder.Entity<AppUserRole>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
            entity.HasMany(e => e.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Sku).HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
            entity.Property(e => e.Total).HasPrecision(10, 2);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MessageType).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.TenantId).HasMaxLength(50);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);
            entity.HasIndex(e => e.ProcessedOnUtc);
        });

        modelBuilder.Entity<ProcessedMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConsumerName).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => new { e.MessageId, e.ConsumerName }).IsUnique();
        });

        // Cấu hình bảng Role, RolePermission, UserRole
        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();

            entity.HasMany(e => e.Permissions)
                .WithOne(p => p.Role)
                .HasForeignKey(p => p.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppRolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PermissionCode).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.RoleId, e.PermissionCode }).IsUnique();
        });

        modelBuilder.Entity<AppUserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.TenantId, e.UserId, e.RoleId }).IsUnique();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentTenant = _tenantProvider.TenantId;
        var currentUser = _currentUserProvider.UserId;

        // 1. Quản lý Audit & Soft-delete
        var domainEntities = new List<BaseAuditableEntity>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is BaseAuditableEntity auditable)
            {
                domainEntities.Add(auditable);

                switch (entry.State)
                {
                    case EntityState.Added:
                        if (string.IsNullOrWhiteSpace(auditable.TenantId))
                        {
                            auditable.TenantId = currentTenant;
                        }
                        auditable.CreatedAt = DateTime.UtcNow;
                        auditable.CreatedBy = currentUser;
                        auditable.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        auditable.UpdatedAt = DateTime.UtcNow;
                        auditable.UpdatedBy = currentUser;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        auditable.IsDeleted = true;
                        auditable.DeletedAt = DateTime.UtcNow;
                        auditable.DeletedBy = currentUser;
                        break;
                }
            }
        }

        // 2. Transactional Outbox: Tự động gom toàn bộ Domain Events và tạo OutboxMessage trong cùng Transaction
        var outboxMessages = new List<OutboxMessage>();
        foreach (var entity in domainEntities)
        {
            if (entity.DomainEvents.Count != 0)
            {
                foreach (var domainEvent in entity.DomainEvents)
                {
                    outboxMessages.Add(new OutboxMessage
                    {
                        Id = domainEvent.EventId,
                        MessageType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName ?? "UnknownEvent",
                        Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                        OccurredOnUtc = domainEvent.OccurredOnUtc,
                        TenantId = string.IsNullOrWhiteSpace(entity.TenantId) ? currentTenant : entity.TenantId,
                        CorrelationId = Guid.NewGuid().ToString("N")
                    });
                }

                entity.ClearDomainEvents();
            }
        }

        if (outboxMessages.Count != 0)
        {
            OutboxMessages.AddRange(outboxMessages);
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
