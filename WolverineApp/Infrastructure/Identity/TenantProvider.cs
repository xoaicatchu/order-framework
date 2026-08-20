using Microsoft.AspNetCore.Http;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Identity;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

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

                throw new UnauthorizedAccessException("Authenticated requests must contain a tenant_id claim.");
            }

            // Startup migrations/seeding and explicitly system-owned background work run without HTTP context.
            return "system";
        }
    }

    public bool IsHttpRequest => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}
