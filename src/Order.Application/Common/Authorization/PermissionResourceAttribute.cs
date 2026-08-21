namespace WolverineApp.Application.Common.Authorization;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PermissionResourceAttribute : Attribute
{
    public string Resource { get; }
    public string Module { get; }

    public PermissionResourceAttribute(string resource, string module = "General")
    {
        Resource = resource;
        Module = module;
    }
}
