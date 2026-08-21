using WolverineApp.Domain.Common;

namespace WolverineApp.Infrastructure.Persistence.Models;

public class HttpIdempotencyRecord : IMultiTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public string Status { get; set; } = "Processing";
    public int? ResponseStatusCode { get; set; }
    public string? ResponseContentType { get; set; }
    public string? ResponseBody { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
