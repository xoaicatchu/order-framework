using Microsoft.AspNetCore.Authorization;

namespace WolverineApp.Application.Common.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public string Resource { get; }
    public string Action { get; }

    public HasPermissionAttribute(string resource, string action)
    {
        Resource = resource;
        Action = action;
        Policy = $"{resource}:{action}";
    }

    public HasPermissionAttribute(string code)
    {
        var parts = code.Split(':');
        Resource = parts.Length == 2 ? parts[0] : "General";
        Action = parts.Length == 2 ? parts[1] : code;
        Policy = code;
    }
}
