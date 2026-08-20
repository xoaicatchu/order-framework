using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Data Source=orders.db";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new ApplicationDbContext(
            options,
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
