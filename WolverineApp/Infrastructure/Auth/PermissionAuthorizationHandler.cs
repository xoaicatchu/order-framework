using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Identity;

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

        // 1. Kiểm tra nếu là Root User (System Admin tối cao) qua Claim
        var isRootClaim = context.User.FindFirst("is_root")?.Value;
        if (string.Equals(isRootClaim, "true", StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        // 2. Kiểm tra nếu Token đã mang sẵn quyền này hoặc quyền System.Root
        var tokenPermissions = context.User.FindAll("permission").Select(c => c.Value);
        if (tokenPermissions.Contains(SystemPermissions.SystemRoot, StringComparer.OrdinalIgnoreCase) ||
            tokenPermissions.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        // 3. Trích xuất trực tiếp UserId và TenantId từ ClaimsPrincipal
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value
            ?? context.User.Identity?.Name
            ?? "system";

        var tenantId = context.User.FindFirst("tenant_id")?.Value
            ?? "default-tenant";

        // 4. Truy vấn Dynamic RBAC từ Database / Cache theo Tenant
        using var scope = _serviceProvider.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

        if (await permissionService.HasPermissionAsync(userId, tenantId, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
