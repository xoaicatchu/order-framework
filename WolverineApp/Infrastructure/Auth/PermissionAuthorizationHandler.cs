using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Auth;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    public const string RootPermissionCode = "System:Root";
    private readonly IServiceProvider _serviceProvider;

    public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var isRootClaim = context.User.FindFirst("is_root")?.Value;
        if (string.Equals(isRootClaim, "true", StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        var tokenPermissions = context.User.FindAll("permission").Select(c => c.Value);
        if (tokenPermissions.Contains(RootPermissionCode, StringComparer.OrdinalIgnoreCase) ||
            tokenPermissions.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value
            ?? context.User.Identity?.Name
            ?? "system";

        var tenantId = context.User.FindFirst("tenant_id")?.Value
            ?? "default-tenant";

        using var scope = _serviceProvider.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

        if (await permissionService.HasPermissionAsync(userId, tenantId, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
