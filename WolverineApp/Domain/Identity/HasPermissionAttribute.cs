using Microsoft.AspNetCore.Authorization;

namespace WolverineApp.Domain.Identity;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class HasPermissionAttribute : AuthorizeAttribute
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
        if (parts.Length == 2)
        {
            Resource = parts[0];
            Action = parts[1];
        }
        else
        {
            Resource = "General";
            Action = code;
        }
        Policy = code;
    }
}
