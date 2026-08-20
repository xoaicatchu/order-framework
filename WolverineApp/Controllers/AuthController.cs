using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Models;
using WolverineApp.Infrastructure.Auth;

namespace WolverineApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public AuthController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [AllowAnonymous]
    [HttpPost("token")]
    public IActionResult GenerateToken()
    {
        return StatusCode(
            StatusCodes.Status410Gone,
            ApiResponse<object>.Fail(
                "Local token issuance is disabled. Authenticate with the configured identity provider and send its bearer token.",
                "AUTH_PROVIDER_REQUIRED"));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var isRoot = string.Equals(User.FindFirst("is_root")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.Identity?.Name;
        var tenantId = User.FindFirst("tenant_id")?.Value
            ?? User.FindFirst("tenant")?.Value;

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tenantId))
        {
            return Unauthorized(ApiResponse<object>.Fail("The access token must contain sub and tenant_id claims.", "INVALID_IDENTITY"));
        }

        var permissions = await _permissionService.GetUserPermissionsAsync(userId, tenantId);

        var userInfo = new
        {
            UserId = userId,
            Username = User.Identity?.Name,
            TenantId = tenantId,
            IsRoot = isRoot,
            Permissions = permissions,
            Claims = claims
        };

        return Ok(ApiResponse<object>.Ok(userInfo));
    }
}
