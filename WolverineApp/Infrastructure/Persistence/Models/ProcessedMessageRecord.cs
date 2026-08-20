namespace WolverineApp.Infrastructure.Persistence.Models;

public class ProcessedMessageRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MessageId { get; set; }
    public string ConsumerName { get; set; } = string.Empty;
    public DateTime ProcessedOnUtc { get; set; } = DateTime.UtcNow;
}
