using Shouldly;
using WolverineApp.Infrastructure.Options;
using Xunit;

namespace Order.Infrastructure.IntegrationTests;

public sealed class OutboxOptionsTests
{
    [Fact]
    public void OutboxOptionsClampOperationalIntervals()
    {
        var options = new OutboxOptions
        {
            LeaseSeconds = 1,
            FallbackScanIntervalSeconds = 1
        };

        options.LeaseDuration.ShouldBe(TimeSpan.FromSeconds(30));
        options.FallbackScanInterval.ShouldBe(TimeSpan.FromMinutes(1));
    }
}
