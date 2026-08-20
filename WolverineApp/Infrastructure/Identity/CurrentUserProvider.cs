using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Identity;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var idClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.Identity.Name;

                if (!string.IsNullOrWhiteSpace(idClaim))
                    return idClaim;

                throw new UnauthorizedAccessException("Authenticated requests must contain a sub claim.");
            }

            return "system";
        }
    }
}
