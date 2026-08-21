using System.Diagnostics.CodeAnalysis;
using WolverineApp.Domain.Common;

namespace WolverineApp.Domain.Identity;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Permission is a domain concept and the public name is retained for data compatibility.")]
public class AppPermission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsAutoDiscovered { get; set; } = true;
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static AppPermission Create(string resource, string action, string module = "General", bool isAutoDiscovered = true, bool isSystem = false)
    {
        return new AppPermission
        {
            Id = Guid.NewGuid(),
            Code = $"{resource}:{action}",
            Module = module,
            Resource = resource,
            Action = action,
            IsAutoDiscovered = isAutoDiscovered,
            IsSystem = isSystem,
            CreatedAt = DateTime.UtcNow
        };
    }
}
