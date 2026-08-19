using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WolverineApp.Infrastructure.Health;

public class SystemMemoryHealthCheck : IHealthCheck
{
    private const long ThresholdBytes = 1024L * 1024L * 1024L; // 1 GB RAM warning threshold

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var workingSet = Process.GetCurrentProcess().WorkingSet64;

        var data = new Dictionary<string, object>
        {
            { "AllocatedMemoryMB", Math.Round(allocated / 1024.0 / 1024.0, 2) },
            { "ProcessWorkingSetMB", Math.Round(workingSet / 1024.0 / 1024.0, 2) },
            { "Gen0Collections", GC.CollectionCount(0) },
            { "Gen1Collections", GC.CollectionCount(1) },
            { "Gen2Collections", GC.CollectionCount(2) }
        };

        if (workingSet > ThresholdBytes)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Bộ nhớ ứng dụng vượt ngưỡng ({Math.Round(workingSet / 1024.0 / 1024.0, 2)} MB)",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Bộ nhớ ổn định ({Math.Round(workingSet / 1024.0 / 1024.0, 2)} MB)",
            data: data));
    }
}
