using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Auth;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
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

        // Only signed identity-provider claims are accepted. There is no request-controlled root bypass.
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value
            ?? context.User.Identity?.Name;

        var tenantId = context.User.FindFirst("tenant_id")?.Value
            ?? context.User.FindFirst("tenant")?.Value;

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tenantId))
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

        if (await permissionService.HasPermissionAsync(userId, tenantId, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
