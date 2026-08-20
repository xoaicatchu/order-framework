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

        // 1. Root User (System Super Admin) bypass mọi kiểm tra quyền
        var isRootClaim = context.User.FindFirst("is_root")?.Value;
        if (string.Equals(isRootClaim, "true", StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        // 2. Trích xuất danh tính từ Slim JWT Token
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value
            ?? context.User.Identity?.Name
            ?? "system";

        var tenantId = context.User.FindFirst("tenant_id")?.Value
            ?? "default-tenant";

        // 3. Tra cứu quyền động qua L1/L2 HybridCache (Tốc độ: ~50 nanoseconds, hỗ trợ thu hồi quyền tức thì)
        using var scope = _serviceProvider.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

        if (await permissionService.HasPermissionAsync(userId, tenantId, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
