using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var provider = Environment.GetEnvironmentVariable("Database__Provider") ?? "postgresql";
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MigrationConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? (provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase)
                ? "Data Source=orders.dev.db"
                : "Host=localhost;Database=order_framework;Username=postgres");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        if (provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(connectionString);
        }
        else
        {
            optionsBuilder.UseNpgsql(connectionString);
        }

        return new ApplicationDbContext(
            optionsBuilder.Options,
            new DesignTimeTenantProvider(),
            new DesignTimeCurrentUserProvider(),
            new DesignTimeOutboxSignal());
    }

    private sealed class DesignTimeTenantProvider : ITenantProvider
    {
        public string TenantId => "system";
        public bool IsHttpRequest => false;
    }

    private sealed class DesignTimeCurrentUserProvider : ICurrentUserProvider
    {
        public string UserId => "system";
    }

    private sealed class DesignTimeOutboxSignal : IOutboxSignal
    {
        public void Signal() { }
        public Task WaitAsync(CancellationToken cancellationToken) => Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
