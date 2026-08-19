using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.DTOs.Auth;

namespace WolverineApp.Infrastructure.Services;

public class AuthTokenService : IAuthTokenService
{
    private readonly IConfiguration _configuration;

    public AuthTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenResponse GenerateToken(string userId, string tenantId, string role, IEnumerable<string>? permissions = null)
    {
        var secretKey = _configuration["Jwt:SecretKey"] ?? "ThisIsASecretKeyForJwtAuthenticationInEnterpriseSystem123456!";
        var issuer = _configuration["Jwt:Issuer"] ?? "EnterpriseDistributedCore";
        var audience = _configuration["Jwt:Audience"] ?? "EnterpriseDistributedCoreClients";
        var lifetimeMinutes = int.TryParse(_configuration["Jwt:TokenLifetimeMinutes"], out var mins) ? mins : 120;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userId),
            new(ClaimTypes.Role, role),
            new("tenant_id", tenantId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (permissions != null)
        {
            foreach (var perm in permissions)
            {
                claims.Add(new Claim("permission", perm));
            }
        }

        var expires = DateTime.UtcNow.AddMinutes(lifetimeMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);

        return new TokenResponse(
            AccessToken: tokenString,
            TokenType: "Bearer",
            ExpiresInSeconds: lifetimeMinutes * 60,
            UserId: userId,
            Role: role,
            TenantId: tenantId
        );
    }
}
