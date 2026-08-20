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
    public const string RootPermissionCode = "System:Root";
    private readonly IConfiguration _configuration;

    public AuthTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenResponse GenerateToken(string userId, string tenantId, bool isRoot, IEnumerable<string>? permissions = null)
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
            new("tenant_id", tenantId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var permissionList = permissions?.ToList() ?? [];

        if (isRoot)
        {
            claims.Add(new Claim("is_root", "true"));
            claims.Add(new Claim(ClaimTypes.Role, "SystemAdmin"));
            claims.Add(new Claim("permission", RootPermissionCode));
            if (!permissionList.Contains(RootPermissionCode))
            {
                permissionList.Add(RootPermissionCode);
            }
        }

        foreach (var perm in permissionList)
        {
            claims.Add(new Claim("permission", perm));
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
            TenantId: tenantId,
            IsRoot: isRoot,
            Permissions: permissionList
        );
    }
}
