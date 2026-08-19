using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.DTOs.AuditLogs;
using WolverineApp.Application.Queries.AuditLogs.GetAuditLogs;
using WolverineApp.Domain.Common;

namespace WolverineApp.Controllers;

[Authorize(Policy = Permissions.AuditLogs.Read)]
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IMessageBus _bus;

    public AuditLogsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// [Query] Lấy danh sách lịch sử Audit Logs có phân trang (Yêu cầu quyền Admin)
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var response = await _bus.InvokeAsync<PagedResult<AuditLogDto>>(new GetAuditLogsQuery(pageIndex, pageSize));
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(response));
    }
}
