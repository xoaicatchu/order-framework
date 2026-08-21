using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Infrastructure.Options;
using WolverineApp.Infrastructure.Persistence.Models;

namespace WolverineApp.Infrastructure.Health;

public sealed class OutboxHealthCheck : IHealthCheck
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly OutboxOptions _options;

    public OutboxHealthCheck(IUnitOfWork unitOfWork, IOptions<OutboxOptions> options)
    {
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var deadLetterCount = await _unitOfWork.GetRepository<OutboxRecord>().Query()
            .CountAsync(message => message.ProcessedOnUtc == null && message.RetryCount >= _options.MaxRetries, cancellationToken);

        return deadLetterCount == 0
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy($"{deadLetterCount} outbox message(s) exhausted retries.");
    }
}
