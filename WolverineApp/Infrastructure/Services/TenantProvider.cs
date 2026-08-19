using Microsoft.AspNetCore.Http;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public const string DefaultTenantId = "default-tenant";
    public const string TenantHeaderName = "X-Tenant-Id";

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string TenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = httpContext.User.FindFirst("tenant_id")?.Value
                    ?? httpContext.User.FindFirst("tenant")?.Value;

                if (!string.IsNullOrWhiteSpace(tenantClaim))
                    return tenantClaim;
            }

            if (httpContext is not null && httpContext.Request.Headers.TryGetValue(TenantHeaderName, out var tenantValue) && !string.IsNullOrWhiteSpace(tenantValue))
            {
                return tenantValue.ToString();
            }

            return DefaultTenantId;
        }
    }
}
