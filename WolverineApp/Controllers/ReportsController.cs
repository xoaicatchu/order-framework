using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.Common.Reporting;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Application.Queries.Orders.GetOrderById;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[PermissionResource("Reports", "Analytics")]
public class ReportsController : ControllerBase
{
    private readonly IReportEngine _reportEngine;
    private readonly IReportTemplateStore _templateStore;
    private readonly IMessageBus _bus;

    public ReportsController(
        IReportEngine reportEngine,
        IReportTemplateStore templateStore,
        IMessageBus bus)
    {
        _reportEngine = reportEngine;
        _templateStore = templateStore;
        _bus = bus;
    }

    [HttpGet("templates")]
    [HasPermission("Reports", "Read")]
    public async Task<IActionResult> GetAvailableTemplates()
    {
        var tenantId = User.FindFirst("tenant_id")?.Value ?? "default-tenant";
        var templates = await _templateStore.ListAvailableTemplatesAsync(tenantId);
        return Ok(ApiResponse<List<string>>.Ok(templates));
    }

    [HttpPost("render")]
    [HasPermission("Reports", "Export")]
    public async Task<IActionResult> RenderReport([FromBody] ReportRenderRequest request)
    {
        var result = await _reportEngine.RenderAsync(request);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet("orders/{orderId:guid}/print")]
    [HasPermission("Reports", "Export")]
    public async Task<IActionResult> PrintOrderInvoice(
        Guid orderId,
        [FromQuery] ReportOutputFormat format = ReportOutputFormat.Pdf)
    {
        var order = await _bus.InvokeAsync<OrderDto>(new GetOrderByIdQuery(orderId));
        if (order is null)
        {
            return NotFound(ApiResponse<OrderDto>.Fail("Order not found."));
        }

        var request = new ReportRenderRequest(
            TemplateCode: "Invoice_A4",
            DataModel: order,
            Format: format
        );

        var result = await _reportEngine.RenderAsync(request);
        return File(result.Content, result.ContentType, result.FileName);
    }
}
