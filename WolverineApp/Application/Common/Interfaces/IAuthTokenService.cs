using WolverineApp.Application.DTOs.Auth;

namespace WolverineApp.Application.Common.Interfaces;

public interface IAuthTokenService
{
    TokenResponse GenerateToken(string userId, string tenantId, string role, IEnumerable<string>? permissions = null);
}
