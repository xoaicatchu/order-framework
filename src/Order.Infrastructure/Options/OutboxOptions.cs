namespace WolverineApp.Infrastructure.Options;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int BatchSize { get; set; } = 50;
    public int MaxRetries { get; set; } = 8;
    public int LeaseSeconds { get; set; } = 120;
    public int FallbackScanIntervalSeconds { get; set; } = 300;

    public TimeSpan LeaseDuration => TimeSpan.FromSeconds(Math.Clamp(LeaseSeconds, 30, 3600));
    public TimeSpan FallbackScanInterval => TimeSpan.FromSeconds(Math.Clamp(FallbackScanIntervalSeconds, 60, 3600));
}
