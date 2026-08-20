using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using WolverineApp.Application.Commands.Roles.AssignUserRoles;
using WolverineApp.Application.Commands.Roles.CreateRole;
using WolverineApp.Application.Commands.Roles.DeleteRole;
using WolverineApp.Application.Commands.Roles.UpdateRole;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.DTOs.Roles;
using WolverineApp.Application.Queries.Roles.GetPermissions;
using WolverineApp.Application.Queries.Roles.GetRoleById;
using WolverineApp.Application.Queries.Roles.GetRoles;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IMessageBus _bus;

    public RolesController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// [UI Query] Lấy danh mục tất cả quyền hạn có sẵn trong hệ thống phân theo Module để hiển thị cây phân quyền checkbox trên UI
    /// </summary>
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        var response = await _bus.InvokeAsync<List<PermissionDefinition>>(new GetPermissionsQuery());
        return Ok(ApiResponse<List<PermissionDefinition>>.Ok(response));
    }

    /// <summary>
    /// [Query] Lấy danh sách các vai trò động được định nghĩa trong đơn vị hiện tại
    /// </summary>
    [HttpGet]
    [Authorize(Policy = SystemPermissions.RolesRead)]
    public async Task<IActionResult> GetRoles()
    {
        var response = await _bus.InvokeAsync<List<RoleDto>>(new GetRolesQuery());
        return Ok(ApiResponse<List<RoleDto>>.Ok(response));
    }

    /// <summary>
    /// [Query] Lấy chi tiết một vai trò và danh sách quyền hạn đã gán
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = SystemPermissions.RolesRead)]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        var response = await _bus.InvokeAsync<RoleDto>(new GetRoleByIdQuery(id));
        return Ok(ApiResponse<RoleDto>.Ok(response));
    }

    /// <summary>
    /// [Command] Tạo vai trò động mới cho đơn vị kèm danh sách quyền được chọn từ UI
    /// </summary>
    [HttpPost]
    [Authorize(Policy = SystemPermissions.RolesCreate)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var command = new CreateRoleCommand(request.Name, request.Description, request.Permissions);
        var response = await _bus.InvokeAsync<RoleDto>(command);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RoleDto>.Created(response, "Tạo vai trò thành công."));
    }

    /// <summary>
    /// [Command] Cập nhật vai trò và cấu hình lại danh sách quyền của đơn vị
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = SystemPermissions.RolesUpdate)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var command = new UpdateRoleCommand(id, request.Name, request.Description, request.Permissions);
        var response = await _bus.InvokeAsync<RoleDto>(command);
        return Ok(ApiResponse<RoleDto>.Ok(response, "Cập nhật vai trò thành công."));
    }

    /// <summary>
    /// [Command] Xóa vai trò tùy biến của đơn vị
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SystemPermissions.RolesDelete)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        await _bus.InvokeAsync<bool>(new DeleteRoleCommand(id));
        return Ok(ApiResponse<bool>.Ok(true, "Xóa vai trò thành công."));
    }

    /// <summary>
    /// [Command] Gán các vai trò động cho người dùng trong đơn vị
    /// </summary>
    [HttpPost("assign")]
    [Authorize(Policy = SystemPermissions.RolesAssign)]
    public async Task<IActionResult> AssignUserRoles([FromBody] AssignUserRolesRequest request)
    {
        var command = new AssignUserRolesCommand(request.UserId, request.RoleIds);
        await _bus.InvokeAsync<bool>(command);
        return Ok(ApiResponse<bool>.Ok(true, "Phân quyền vai trò cho người dùng thành công."));
    }
}
