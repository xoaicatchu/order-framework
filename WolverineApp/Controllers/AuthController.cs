using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.DTOs.Auth;
using WolverineApp.Domain.Common;

namespace WolverineApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthTokenService _authTokenService;

    public AuthController(IAuthTokenService authTokenService)
    {
        _authTokenService = authTokenService;
    }

    /// <summary>
    /// [Auth] Tạo JWT Bearer Token đăng nhập phục vụ phát triển và kiểm thử
    /// </summary>
    [AllowAnonymous]
    [HttpPost("token")]
    public IActionResult GenerateToken([FromBody] LoginRequest request)
    {
        var role = request.Role ?? Roles.Manager;
        var tenantId = request.TenantId ?? "default-tenant";

        // Gán các permissions mặc định theo Role
        var permissions = new List<string>();
        if (role is Roles.Admin or Roles.Manager or Roles.Operator or Roles.Viewer)
        {
            permissions.Add(Permissions.Orders.Read);
        }
        if (role is Roles.Admin or Roles.Manager or Roles.Operator)
        {
            permissions.Add(Permissions.Orders.Create);
            permissions.Add(Permissions.Orders.Update);
        }
        if (role is Roles.Admin or Roles.Manager)
        {
            permissions.Add(Permissions.Orders.Cancel);
        }
        if (role is Roles.Admin)
        {
            permissions.Add(Permissions.AuditLogs.Read);
        }

        var response = _authTokenService.GenerateToken(request.Username, tenantId, role, permissions);
        return Ok(ApiResponse<TokenResponse>.Ok(response, "Đăng nhập thành công."));
    }

    /// <summary>
    /// [Auth] Lấy thông tin tài khoản hiện tại từ JWT Token
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var userInfo = new
        {
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Username = User.Identity?.Name,
            Role = User.FindFirst(ClaimTypes.Role)?.Value,
            TenantId = User.FindFirst("tenant_id")?.Value,
            Claims = claims
        };

        return Ok(ApiResponse<object>.Ok(userInfo));
    }
}
