namespace WolverineApp.Domain.Identity;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PermissionResourceAttribute : Attribute
{
    public string Resource { get; set; }
    public string Module { get; set; } = "General";

    public PermissionResourceAttribute(string resource)
    {
        Resource = resource;
    }

    public PermissionResourceAttribute(string resource, string module)
    {
        Resource = resource;
        Module = module;
    }
}
