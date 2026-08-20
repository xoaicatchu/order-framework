using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.Common.Reporting;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Application.Queries.Orders.GetOrderById;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Controllers;

public record SaveTemplateDto(string TemplateCode, string Content);

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

    [HttpGet("templates/{code}")]
    [HasPermission("Reports", "Read")]
    public async Task<IActionResult> GetTemplateContent(string code)
    {
        var tenantId = User.FindFirst("tenant_id")?.Value ?? "default-tenant";
        var content = await _templateStore.GetTemplateContentAsync(code, tenantId);
        if (content is null)
        {
            return NotFound(ApiResponse<string>.Fail($"Không tìm thấy mẫu in '{code}'."));
        }
        return Ok(ApiResponse<string>.Ok(content));
    }

    [HttpPost("templates")]
    [HasPermission("Reports", "Export")]
    public async Task<IActionResult> SaveTemplate([FromBody] SaveTemplateDto dto)
    {
        var tenantId = User.FindFirst("tenant_id")?.Value ?? "default-tenant";

        var validation = _reportEngine.ValidateTemplate(dto.Content);
        if (!validation.IsValid)
        {
            return BadRequest(ApiResponse<string>.Fail(validation.ErrorMessage ?? "Lỗi cú pháp Liquid.", "SYNTAX_ERROR"));
        }

        await _templateStore.SaveCustomTemplateAsync(dto.TemplateCode, tenantId, dto.Content);
        return Ok(ApiResponse<string>.Ok($"Đã lưu mẫu in '{dto.TemplateCode}' vào Database thành công cho đơn vị '{tenantId}'."));
    }

    [HttpDelete("templates/{code}")]
    [HasPermission("Reports", "Export")]
    public async Task<IActionResult> DeleteCustomTemplate(string code)
    {
        var tenantId = User.FindFirst("tenant_id")?.Value ?? "default-tenant";
        await _templateStore.DeleteCustomTemplateAsync(code, tenantId);
        return Ok(ApiResponse<string>.Ok($"Đã xóa mẫu in tùy biến '{code}' của đơn vị '{tenantId}'."));
    }

    [HttpPost("templates/validate")]
    [HasPermission("Reports", "Read")]
    public IActionResult ValidateTemplate([FromBody] ValidateTemplateRequest request)
    {
        var result = _reportEngine.ValidateTemplate(request.TemplateContent);
        return Ok(ApiResponse<TemplateValidationResult>.Ok(result));
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
