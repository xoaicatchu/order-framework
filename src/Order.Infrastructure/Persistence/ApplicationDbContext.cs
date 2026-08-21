using System.Text.Json;
using System.Security;
using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Audit;
using WolverineApp.Domain.Common;
using WolverineApp.Domain.Identity;
using WolverineApp.Domain.Orders;
using WolverineApp.Infrastructure.Persistence.Models;

namespace WolverineApp.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IOutboxSignal _outboxSignal;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider tenantProvider,
        ICurrentUserProvider currentUserProvider,
        IOutboxSignal outboxSignal)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _currentUserProvider = currentUserProvider;
        _outboxSignal = outboxSignal;
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxRecord> OutboxRecords => Set<OutboxRecord>();
    public DbSet<ProcessedMessageRecord> ProcessedMessageRecords => Set<ProcessedMessageRecord>();
    public DbSet<HttpIdempotencyRecord> HttpIdempotencyRecords => Set<HttpIdempotencyRecord>();
    public DbSet<TenantMembershipRecord> TenantMemberships => Set<TenantMembershipRecord>();

    // Dynamic RBAC Entities
    public DbSet<AppPermission> Permissions => Set<AppPermission>();
    public DbSet<AppRole> Roles => Set<AppRole>();
    public DbSet<AppRolePermission> RolePermissions => Set<AppRolePermission>();
    public DbSet<AppUserRole> UserRoles => Set<AppUserRole>();

    // Report Templates & Semantic Reporting
    public DbSet<WolverineApp.Domain.Reporting.ReportTemplate> ReportTemplates => Set<WolverineApp.Domain.Reporting.ReportTemplate>();
    public DbSet<WolverineApp.Domain.Reporting.SemanticDataset> SemanticDatasets => Set<WolverineApp.Domain.Reporting.SemanticDataset>();
    public DbSet<WolverineApp.Domain.Reporting.ReportConfiguration> ReportConfigurations => Set<WolverineApp.Domain.Reporting.ReportConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Query Filters for Soft Delete & Multi-Tenancy
        modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<TenantMembershipRecord>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        
        // Multi-Tenancy on Dynamic Roles & Report Templates & Configurations
        modelBuilder.Entity<AppRole>().HasQueryFilter(e => !e.IsDeleted && (e.TenantId == _tenantProvider.TenantId || e.IsSystemRole));
        modelBuilder.Entity<AppRolePermission>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId && !e.Role.IsDeleted);
        modelBuilder.Entity<AppUserRole>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId && !e.Role.IsDeleted);
        modelBuilder.Entity<WolverineApp.Domain.Reporting.ReportTemplate>().HasQueryFilter(e => !e.IsDeleted && (e.TenantId == _tenantProvider.TenantId || e.IsSystemDefault));
        modelBuilder.Entity<WolverineApp.Domain.Reporting.SemanticDataset>().HasQueryFilter(e => !e.IsDeleted && e.IsActive && (e.TenantId == _tenantProvider.TenantId || e.TenantId == "system"));
        modelBuilder.Entity<WolverineApp.Domain.Reporting.ReportConfiguration>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<WolverineApp.Domain.Reporting.ReportTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
        });

        modelBuilder.Entity<WolverineApp.Domain.Reporting.SemanticDataset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FieldsMetadataJson).IsRequired();
            entity.Property(e => e.BaseQuerySql).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<WolverineApp.Domain.Reporting.ReportConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.DatasetCode).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SelectedFieldsJson).IsRequired();
            entity.Property(e => e.FilterConfigJson).IsRequired();
            entity.Property(e => e.TemplateContent).IsRequired();
            entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
        });

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
            entity.HasIndex(e => new { e.TenantId, e.CreatedAt });
            entity.HasIndex(e => new { e.TenantId, e.Status, e.CreatedAt });
            entity.HasIndex(e => new { e.TenantId, e.OrderNumber }).IsUnique();
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
            entity.HasIndex(e => new { e.TenantId, e.Timestamp });
        });

        modelBuilder.Entity<OutboxRecord>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MessageType).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.TenantId).HasMaxLength(50);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);
            entity.Property(e => e.LockOwner).HasMaxLength(100);
            entity.HasIndex(e => new { e.ProcessedOnUtc, e.NextAttemptAtUtc, e.LockedUntilUtc });
        });

        modelBuilder.Entity<ProcessedMessageRecord>(entity =>
        {
            entity.ToTable("ProcessedMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConsumerName).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => new { e.MessageId, e.ConsumerName }).IsUnique();
        });

        modelBuilder.Entity<HttpIdempotencyRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Method).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RequestHash).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ResponseContentType).HasMaxLength(200);
            entity.HasIndex(e => new { e.TenantId, e.UserId, e.Method, e.Path, e.IdempotencyKey }).IsUnique();
            entity.HasIndex(e => e.ExpiresAtUtc);
        });

        modelBuilder.Entity<TenantMembershipRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.TenantId, e.UserId }).IsUnique();
        });

        // Cấu hình bảng AppPermission (Danh mục quyền hạn động hệ thống)
        modelBuilder.Entity<AppPermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Module).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Resource).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentTenant = _tenantProvider.TenantId;
        var currentUser = _currentUserProvider.UserId;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IMultiTenant tenantEntity)
                continue;

            if (entry.State == EntityState.Added && string.IsNullOrWhiteSpace(tenantEntity.TenantId))
            {
                tenantEntity.TenantId = currentTenant;
            }

            if (_tenantProvider.IsHttpRequest
                && !string.Equals(tenantEntity.TenantId, currentTenant, StringComparison.Ordinal))
            {
                throw new SecurityException($"Cross-tenant write rejected. Expected '{currentTenant}'.");
            }
        }

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

        // 2. Transactional Outbox: Tự động gom toàn bộ Domain Events và tạo OutboxRecord trong cùng Transaction
        var outboxMessages = new List<OutboxRecord>();
        foreach (var entity in domainEntities)
        {
            if (entity.DomainEvents.Count != 0)
            {
                foreach (var domainEvent in entity.DomainEvents)
                {
                    outboxMessages.Add(new OutboxRecord
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
            OutboxRecords.AddRange(outboxMessages);
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        if (outboxMessages.Count != 0)
        {
            _outboxSignal.Signal();
        }

        return result;
    }
}
