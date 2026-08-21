using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Reporting;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Application.Queries.Orders.GetOrderById;
using WolverineApp.Application.Common.Authorization;
using WolverineApp.Domain.Reporting;

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

// Public report contract: consumers should use catalog -> create -> export.
// The configuration/template terminology remains available for backward compatibility.
public record ReportCatalogDto(List<ReportDataSourceDto> DataSources);

public record ReportDataSourceDto(
    string Id,
    string Name,
    string Category,
    string? Description,
    List<ReportFieldDto> Fields
);

public record ReportFieldDto(
    string Id,
    string Name,
    string Type,
    bool CanFilter,
    List<string>? Options
);

public record ReportFilterDefinition(
    string Field,
    string Type,
    string? Label = null,
    bool Required = false,
    string? DefaultValue = null
);

public record CreateReportRequest(
    string Name,
    string DataSourceId,
    List<string>? Columns = null,
    List<ReportFilterDefinition>? Filters = null,
    string? Code = null
);

public record ReportCreatedDto(string Code, string Name, string DataSourceId);

public record ExportReportRequest(
    string Format = "pdf",
    Dictionary<string, JsonElement>? Filters = null
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _bus;
    private readonly ITenantProvider _tenantProvider;

    public ReportsController(
        IReportEngine reportEngine,
        IReportTemplateStore templateStore,
        ISemanticDatasetService semanticService,
        IUnitOfWork unitOfWork,
        IMessageBus bus,
        ITenantProvider tenantProvider)
    {
        _reportEngine = reportEngine;
        _templateStore = templateStore;
        _semanticService = semanticService;
        _unitOfWork = unitOfWork;
        _bus = bus;
        _tenantProvider = tenantProvider;
    }

    // ==========================================
    // 0. SIMPLE PUBLIC CONTRACT
    // ==========================================

    [HttpGet("catalog")]
    [HasPermission("Reports", "Read")]
    public async Task<IActionResult> GetReportCatalog(CancellationToken cancellationToken)
    {
        var datasets = await _semanticService.GetAvailableDatasetsAsync(cancellationToken);
        var dataSources = datasets.Select(dataset => new ReportDataSourceDto(
            Id: dataset.Code,
            Name: dataset.Name,
            Category: dataset.Category,
            Description: dataset.Description,
            Fields: dataset.Fields.Select(field => new ReportFieldDto(
                Id: field.Key,
                Name: field.Label,
                Type: field.Type,
                CanFilter: field.Filterable,
                Options: field.EnumValues)).ToList())).ToList();

        return Ok(ApiResponse<ReportCatalogDto>.Ok(new ReportCatalogDto(dataSources)));
    }

    [HttpPost]
    [HasPermission("Reports", "Export")]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.DataSourceId))
        {
            return BadRequest(ApiResponse<ReportCreatedDto>.Fail("Tên báo cáo và nguồn dữ liệu là bắt buộc."));
        }

        var code = string.IsNullOrWhiteSpace(request.Code)
            ? $"report-{Guid.NewGuid():N}"
            : request.Code.Trim();

        var filters = (request.Filters ?? [])
            .Select(filter => new ReportFilterConfigItem
            {
                FieldName = filter.Field,
                Label = filter.Label ?? filter.Field,
                FilterType = filter.Type,
                Required = filter.Required,
                DefaultValue = filter.DefaultValue
            })
            .ToList();

        var result = await SaveReportConfiguration(new SaveReportConfigurationRequest(
            Code: code,
            Name: request.Name.Trim(),
            DatasetCode: request.DataSourceId,
            SelectedFields: request.Columns ?? [],
            Filters: filters));

        if (result is not OkObjectResult)
        {
            return result;
        }

        return Ok(ApiResponse<ReportCreatedDto>.Created(
            new ReportCreatedDto(code, request.Name.Trim(), request.DataSourceId),
            "Đã tạo báo cáo."));
    }

    [HttpPost("{code}/export")]
    [HasPermission("Reports", "Export")]
    public async Task<IActionResult> ExportReport(string code, [FromBody] ExportReportRequest request)
    {
        if (!Enum.TryParse<ReportOutputFormat>(request.Format, ignoreCase: true, out var format)
            || format is ReportOutputFormat.Excel or ReportOutputFormat.Csv)
        {
            return BadRequest(ApiResponse<string>.Fail("Định dạng hỗ trợ hiện tại chỉ gồm pdf và html."));
        }

        var criteria = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (field, value) in request.Filters ?? [])
        {
            if (value.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in value.EnumerateObject())
                {
                    criteria[$"{field}_{property.Name}"] = ToObject(property.Value);
                }
            }
            else
            {
                criteria[field] = ToObject(value);
            }
        }

        return await ExecuteConfiguredReport(code, new ExecuteReportConfigRequest(criteria, format));
    }

    private static object? ToObject(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number when value.TryGetInt64(out var integer) => integer,
            JsonValueKind.Number when value.TryGetDecimal(out var decimalValue) => decimalValue,
            JsonValueKind.True or JsonValueKind.False => value.GetBoolean(),
            _ => value.ToString()
        };
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
        var tenantId = _tenantProvider.TenantId;
        var configs = await _unitOfWork.GetRepository<ReportConfiguration>().Query()
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
        var tenantId = _tenantProvider.TenantId;
        var config = await _unitOfWork.GetRepository<ReportConfiguration>().Query()
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
        var tenantId = _tenantProvider.TenantId;

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

        var configurationRepository = _unitOfWork.GetRepository<ReportConfiguration>();
        var existing = await configurationRepository.Query(tracking: true)
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
            await configurationRepository.AddAsync(newConfig);
        }

        // Đồng thời lưu vào IReportTemplateStore để RenderEngine có thể nạp template
        await _templateStore.SaveCustomTemplateAsync(request.Code, tenantId, template);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok($"Đã cấu hình thành công báo cáo '{request.Code}' cho đơn vị '{tenantId}'."));
    }

    [HttpPost("configurations/{code}/execute")]
    [HasPermission("Reports", "Export")]
    public async Task<IActionResult> ExecuteConfiguredReport(string code, [FromBody] ExecuteReportConfigRequest request)
    {
        var tenantId = _tenantProvider.TenantId;

        var config = await _unitOfWork.GetRepository<ReportConfiguration>().Query()
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
            Format: request.Format
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
        var tenantId = _tenantProvider.TenantId;
        var templates = await _templateStore.ListAvailableTemplatesAsync(tenantId);
        return Ok(ApiResponse<List<string>>.Ok(templates));
    }

    [HttpGet("templates/{code}")]
    [HasPermission("Reports", "Read")]
    public async Task<IActionResult> GetTemplateContent(string code)
    {
        var tenantId = _tenantProvider.TenantId;
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
        var tenantId = _tenantProvider.TenantId;

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
        var tenantId = _tenantProvider.TenantId;
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
