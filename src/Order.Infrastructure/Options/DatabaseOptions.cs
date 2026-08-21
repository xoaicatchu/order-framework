namespace WolverineApp.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; set; } = "postgresql";
    public bool AutoMigrate { get; set; }
    public bool SeedDemoData { get; set; }
    public bool RequireExternalMigration { get; set; } = true;
}
