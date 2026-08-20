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
using WolverineApp.Application.Queries.Roles.GetPermissionsMatrix;
using WolverineApp.Application.Queries.Roles.GetRoleById;
using WolverineApp.Application.Queries.Roles.GetRoles;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[PermissionResource("Roles", "IAM")]
public class RolesController : ControllerBase
{
    private readonly IMessageBus _bus;

    public RolesController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpGet("permissions/matrix")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPermissionsMatrix()
    {
        var response = await _bus.InvokeAsync<PermissionMatrixDto>(new GetPermissionsMatrixQuery());
        return Ok(ApiResponse<PermissionMatrixDto>.Ok(response));
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        var response = await _bus.InvokeAsync<List<PermissionDto>>(new GetPermissionsQuery());
        return Ok(ApiResponse<List<PermissionDto>>.Ok(response));
    }

    [HttpGet]
    [HasPermission("Roles", "Read")]
    public async Task<IActionResult> GetRoles()
    {
        var response = await _bus.InvokeAsync<List<RoleDto>>(new GetRolesQuery());
        return Ok(ApiResponse<List<RoleDto>>.Ok(response));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Roles", "Read")]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        var response = await _bus.InvokeAsync<RoleDto>(new GetRoleByIdQuery(id));
        return Ok(ApiResponse<RoleDto>.Ok(response));
    }

    [HttpPost]
    [HasPermission("Roles", "Create")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var command = new CreateRoleCommand(request.Name, request.Description, request.Permissions);
        var response = await _bus.InvokeAsync<RoleDto>(command);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RoleDto>.Created(response, "Role created successfully."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Roles", "Update")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var command = new UpdateRoleCommand(id, request.Name, request.Description, request.Permissions);
        var response = await _bus.InvokeAsync<RoleDto>(command);
        return Ok(ApiResponse<RoleDto>.Ok(response, "Role updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Roles", "Delete")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        await _bus.InvokeAsync<bool>(new DeleteRoleCommand(id));
        return Ok(ApiResponse<bool>.Ok(true, "Role deleted successfully."));
    }

    [HttpPost("assign")]
    [HasPermission("Roles", "Assign")]
    public async Task<IActionResult> AssignUserRoles([FromBody] AssignUserRolesRequest request)
    {
        var command = new AssignUserRolesCommand(request.UserId, request.RoleIds);
        await _bus.InvokeAsync<bool>(command);
        return Ok(ApiResponse<bool>.Ok(true, "User roles assigned successfully."));
    }
}
