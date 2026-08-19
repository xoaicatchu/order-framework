using Microsoft.AspNetCore.Http;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Services;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public const string DefaultUserId = "system";
    public const string UserHeaderName = "X-User-Id";

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is not null && httpContext.Request.Headers.TryGetValue(UserHeaderName, out var userValue) && !string.IsNullOrWhiteSpace(userValue))
            {
                return userValue.ToString();
            }

            return httpContext?.User?.Identity?.Name ?? DefaultUserId;
        }
    }
}
