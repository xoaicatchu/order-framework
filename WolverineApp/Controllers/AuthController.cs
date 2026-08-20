using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.DTOs.Auth;
using WolverineApp.Infrastructure.Auth;

namespace WolverineApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthTokenService _authTokenService;
    private readonly IPermissionService _permissionService;

    public AuthController(
        IAuthTokenService authTokenService,
        IPermissionService permissionService)
    {
        _authTokenService = authTokenService;
        _permissionService = permissionService;
    }

    /// <summary>
    /// [Auth] Đăng nhập và tạo JWT Bearer Token (Hỗ trợ Root User / Admin đơn vị / Nhân viên với quyền động từ Database)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("token")]
    public async Task<IActionResult> GenerateToken([FromBody] LoginRequest request)
    {
        var tenantId = request.TenantId ?? "default-tenant";

        // 1. Nếu là Root User (System Admin tối cao toàn hệ thống)
        if (request.IsRoot)
        {
            var rootResponse = _authTokenService.GenerateToken(
                request.Username,
                "system",
                isRoot: true,
                new[] { PermissionDiscoveryService.RootPermissionCode }
            );
            return Ok(ApiResponse<TokenResponse>.Ok(rootResponse, "Đăng nhập Root User hệ thống thành công."));
        }

        // 2. Nếu có danh sách quyền truyền trực tiếp (dùng cho test nhanh hoặc custom)
        List<string> permissions;
        if (request.Permissions != null && request.Permissions.Count > 0)
        {
            permissions = request.Permissions;
        }
        else
        {
            // Truy vấn quyền động từ Database của đơn vị (Dynamic RBAC)
            permissions = await _permissionService.GetUserPermissionsAsync(request.Username, tenantId);
        }

        var response = _authTokenService.GenerateToken(request.Username, tenantId, isRoot: false, permissions);
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
        var isRoot = string.Equals(User.FindFirst("is_root")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        var permissions = User.FindAll("permission").Select(c => c.Value).ToList();

        var userInfo = new
        {
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Username = User.Identity?.Name,
            TenantId = User.FindFirst("tenant_id")?.Value,
            IsRoot = isRoot,
            Permissions = permissions,
            Claims = claims
        };

        return Ok(ApiResponse<object>.Ok(userInfo));
    }
}
