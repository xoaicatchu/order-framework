using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.DTOs.AuditLogs;
using WolverineApp.Application.Queries.AuditLogs.GetAuditLogs;
using WolverineApp.Application.Common.Authorization;

namespace WolverineApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[PermissionResource("AuditLogs", "Audit")]
public class AuditLogsController : ControllerBase
{
    private readonly IMessageBus _bus;

    public AuditLogsController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpGet("list")]
    [HasPermission("AuditLogs", "Read")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var response = await _bus.InvokeAsync<PagedResult<AuditLogDto>>(new GetAuditLogsQuery(pageIndex, pageSize));
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(response));
    }
}
