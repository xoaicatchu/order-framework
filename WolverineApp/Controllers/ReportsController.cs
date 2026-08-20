using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.Common.Reporting;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Application.Queries.Orders.GetOrderById;
using WolverineApp.Domain.Identity;
using WolverineApp.Domain.Reporting;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Controllers;

public record SaveTemplateDto(string TemplateCode, string Content);

public record SaveReportConfigurationRequest(
    string Code,
    string Name,
    string DatasetCode,
    List<string> SelectedFields,
    List<ReportFilterConfigItem> Filters,
    string? CustomTemplateContent = null
);

public record ExecuteReportConfigRequest(
    Dictionary<string, object?> Criteria,
    ReportOutputFormat Format = ReportOutputFormat.Html
);

[Authorize]
[ApiController]
[Route("api/[controller]")]
[PermissionResource("Reports", "Analytics")]
public class ReportsController : ControllerBase
{
    private readonly IReportEngine _reportEngine;
    private readonly IReportTemplateStore _templateStore;
    private readonly ISemanticDatasetService _semanticService;
    private readonly ApplicationDbContext _dbContext;
    private readonly IMessageBus _bus;

    public ReportsController(
        IReportEngine reportEngine,
        IReportTemplateStore templateStore,
        ISemanticDatasetService semanticService,
        ApplicationDbContext dbContext,
        IMessageBus bus)
    {
        _reportEngine = reportEngine;
        _templateStore = templateStore;
        _semanticService = semanticService;
        _dbContext = dbContext;
        _bus = bus;
    }

    // ==========================================
    // 1. SEMANTIC DATASETS (NGUỒN DỮ LIỆU NGHIỆP VỤ)
    // ==========================================

    [HttpGet("semantic-datasets")]
    [HasPermission("Reports", "Read")]
    public async Task<IActionResult> GetSemanticDatasets()
    {
        var datasets = await _semanticService.GetAvailableDatasetsAsync();
        return Ok(ApiResponse<List<SemanticDatasetDto>>.Ok(datasets));
    }

    [HttpGet("semantic-datasets/{code}")]
    [HasPermission("Reports", "Read")]
    public async Task<IActionResult> GetSemanticDatasetByCode(string code)
    {
        var dataset = await _semanticService.GetDatasetByCodeAsync(code);
        if (dataset is null)
        {
            return NotFound(ApiResponse<SemanticDatasetDto>.Fail($"Không tìm thấy nguồn dữ liệu '{code}'."));
        }
        return Ok(ApiResponse<SemanticDatasetDto>.Ok(dataset));
    }

    // ==========================================
    // 2. REPORT CONFIGURATIONS (CẤU HÌNH BÁO CÁO VISUAL)
    // ==========================================

    [HttpGet("configurations")]
    [HasPermission("Reports", "Read")]
    public async Task<IActionResult> GetReportConfigurations()
    {
        var tenantId = User.FindFirst("tenant_id")?.Value ?? "default-tenant";
        var configs = await _dbContext.ReportConfigurations
            .Where(c => c.TenantId == tenantId && c.IsActive && !c.IsDeleted)
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.Name,
                c.DatasetCode,
                c.Version,
                c.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(configs));
    }

    [HttpGet("configurations/{code}/form-schema")]
    [HasPermission("Reports", "Read")]
    public async Task<IActionResult> GetReportFilterFormSchema(string code)
    {
        var tenantId = User.FindFirst("tenant_id")?.Value ?? "default-tenant";
        var config = await _dbContext.ReportConfigurations
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Code == code && c.IsActive && !c.IsDeleted);

        if (config is null)
        {
            return NotFound(ApiResponse<List<ReportFilterConfigItem>>.Fail($"Không tìm thấy cấu hình báo cáo '{code}'."));
        }

        var filters = JsonSerializer.Deserialize<List<ReportFilterConfigItem>>(config.FilterConfigJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        return Ok(ApiResponse<List<ReportFilterConfigItem>>.Ok(filters));
    }

    [HttpPost("configurations")]
    [HasPermission("Reports", "Export")]
    public async Task<IActionResult> SaveReportConfiguration([FromBody] SaveReportConfigurationRequest request)
    {
        var tenantId = User.FindFirst("tenant_id")?.Value ?? "default-tenant";

        var dataset = await _semanticService.GetDatasetByCodeAsync(request.DatasetCode);
        if (dataset is null)
        {
            return BadRequest(ApiResponse<string>.Fail($"Nguồn dữ liệu nghiệp vụ '{request.DatasetCode}' không tồn tại."));
        }

        var template = request.CustomTemplateContent;
        if (string.IsNullOrWhiteSpace(template))
        {
            var selectedFieldDefs = dataset.Fields
                .Where(f => request.SelectedFields.Contains(f.Key, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (selectedFieldDefs.Count == 0)
            {
                selectedFieldDefs = dataset.Fields;
            }

            template = _semanticService.GenerateDefaultLiquidTemplate(request.Name, selectedFieldDefs);
        }

        var validation = _reportEngine.ValidateTemplate(template);
        if (!validation.IsValid)
        {
            return BadRequest(ApiResponse<string>.Fail($"Lỗi cú pháp mẫu in: {validation.ErrorMessage}"));
        }

        var existing = await _dbContext.ReportConfigurations
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Code == request.Code);

        var selectedJson = JsonSerializer.Serialize(request.SelectedFields);
        var filterJson = JsonSerializer.Serialize(request.Filters);

        if (existing is not null)
        {
            existing.Update(request.Name, selectedJson, filterJson, template, User.Identity?.Name ?? "system");
        }
        else
        {
            var newConfig = ReportConfiguration.Create(
                code: request.Code,
                name: request.Name,
                datasetCode: request.DatasetCode,
                tenantId: tenantId,
                selectedFieldsJson: selectedJson,
                filterConfigJson: filterJson,
                templateContent: template
            );
            await _dbContext.ReportConfigurations.AddAsync(newConfig);
        }

        // Đồng thời lưu vào IReportTemplateStore để RenderEngine có thể nạp template
        await _templateStore.SaveCustomTemplateAsync(request.Code, tenantId, template);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok($"Đã cấu hình thành công báo cáo '{request.Code}' cho đơn vị '{tenantId}'."));
    }

    [HttpPost("configurations/{code}/execute")]
    [HasPermission("Reports", "Export")]
    public async Task<IActionResult> ExecuteConfiguredReport(string code, [FromBody] ExecuteReportConfigRequest request)
    {
        var tenantId = User.FindFirst("tenant_id")?.Value ?? "default-tenant";

        var config = await _dbContext.ReportConfigurations
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Code == code && c.IsActive && !c.IsDeleted);

        if (config is null)
        {
            return NotFound(ApiResponse<string>.Fail($"Không tìm thấy cấu hình báo cáo '{code}'."));
        }

        var selectedFields = JsonSerializer.Deserialize<List<string>>(config.SelectedFieldsJson) ?? [];

        // 1. Thực thi truy vấn dữ liệu từ Semantic Dataset an toàn
        var datasetRows = await _semanticService.ExecuteDatasetQueryAsync(
            datasetCode: config.DatasetCode,
            tenantId: tenantId,
            selectedFields: selectedFields,
            filterCriteria: request.Criteria
        );

        // 2. Chuẩn bị model dữ liệu truyền vào Liquid Engine
        var dataModel = new Dictionary<string, object>
        {
            { "Data", datasetRows },
            { "TotalRows", datasetRows.Count },
            { "ReportName", config.Name },
            { "ExecutedAt", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm") }
        };

        // 3. Render tài liệu theo định dạng yêu cầu (PDF / HTML / Excel)
        var renderRequest = new ReportRenderRequest(
            TemplateCode: config.Code,
            DataModel: dataModel,
            Format: request.Format,
            CustomTenantId: tenantId
        );

        var result = await _reportEngine.RenderAsync(renderRequest);
        return File(result.Content, result.ContentType, result.FileName);
    }

    // ==========================================
    // 3. TEMPLATES & RAW ENGINE APIS
    // ==========================================

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
