using System.Collections.Concurrent;
using System.Text.Json;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Reporting;
using WolverineApp.Infrastructure.Reporting.Helpers;

namespace WolverineApp.Infrastructure.Reporting;

public class LiquidReportEngine : IReportEngine
{
    private static readonly FluidParser Parser = new();
    private static readonly ConcurrentDictionary<string, IFluidTemplate> TemplateCache = new();
    private readonly IReportTemplateStore _templateStore;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEnumerable<IDocumentRenderer> _renderers;
    private readonly ILogger<LiquidReportEngine> _logger;

    public LiquidReportEngine(
        IReportTemplateStore templateStore,
        ITenantProvider tenantProvider,
        IEnumerable<IDocumentRenderer> renderers,
        ILogger<LiquidReportEngine> logger)
    {
        _templateStore = templateStore;
        _tenantProvider = tenantProvider;
        _renderers = renderers;
        _logger = logger;
    }

    public async Task<string> RenderHtmlAsync(string templateCode, object dataModel, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var targetTenant = tenantId ?? _tenantProvider.TenantId;
        var rawTemplate = await _templateStore.GetTemplateContentAsync(templateCode, targetTenant, cancellationToken);

        if (string.IsNullOrWhiteSpace(rawTemplate))
        {
            throw new FileNotFoundException($"Report template '{templateCode}' not found for tenant '{targetTenant}'.");
        }

        var cacheKey = $"{targetTenant}:{templateCode}";
        var template = TemplateCache.GetOrAdd(cacheKey, _ =>
        {
            if (!Parser.TryParse(rawTemplate, out var compiled, out var error))
            {
                throw new InvalidOperationException($"Error compiling Liquid template '{templateCode}': {error}");
            }
            return compiled;
        });

        var context = CreateTemplateContext(dataModel);
        return await template.RenderAsync(context);
    }

    public async Task<ReportRenderResult> RenderAsync(ReportRenderRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = request.CustomTenantId ?? _tenantProvider.TenantId;
        _logger.LogInformation("Rendering report template '{TemplateCode}' in format '{Format}' for tenant '{TenantId}'",
            request.TemplateCode, request.Format, tenantId);

        // 1. Biên dịch và nạp dữ liệu vào HTML Layout
        var compiledHtml = await RenderHtmlAsync(request.TemplateCode, request.DataModel, tenantId, cancellationToken);

        // 2. Tìm renderer phù hợp
        var renderer = _renderers.FirstOrDefault(r => r.SupportedFormat == request.Format);
        if (renderer is null)
        {
            throw new NotSupportedException($"Report output format '{request.Format}' is not supported by any registered renderer.");
        }

        // 3. Kết xuất tài liệu nhị phân (PDF / HTML / Excel)
        var content = await renderer.RenderAsync(request.TemplateCode, compiledHtml, request.DataModel, request.Parameters, cancellationToken);

        var (contentType, extension) = request.Format switch
        {
            ReportOutputFormat.Pdf => ("application/pdf", "pdf"),
            ReportOutputFormat.Html => ("text/html; charset=utf-8", "html"),
            ReportOutputFormat.Excel => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
            ReportOutputFormat.Csv => ("text/csv; charset=utf-8", "csv"),
            _ => ("application/octet-stream", "bin")
        };

        var fileName = $"{request.TemplateCode}_{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";

        return new ReportRenderResult(content, contentType, fileName);
    }

    private static TemplateContext CreateTemplateContext(object dataModel)
    {
        var options = new TemplateOptions();

        // 1. Filter định dạng tiền tệ: {{ amount | format_currency: 'USD' }}
        options.Filters.AddFilter("format_currency", (input, arguments, ctx) =>
        {
            var number = input.ToNumberValue();
            var currency = arguments.At(0).ToStringValue() ?? "USD";
            return new StringValue(currency switch
            {
                "VND" => $"{number:N0} đ",
                "USD" => $"${number:N2}",
                _ => $"{number:N2} {currency}"
            });
        });

        // 2. Filter định dạng ngày tháng: {{ date | format_date: 'dd/MM/yyyy' }}
        options.Filters.AddFilter("format_date", (input, arguments, ctx) =>
        {
            var format = arguments.At(0).ToStringValue() ?? "dd/MM/yyyy";
            if (DateTime.TryParse(input.ToStringValue(), out var dt))
            {
                return new StringValue(dt.ToString(format));
            }
            return input;
        });

        // 3. Filter đọc số tiền thành chữ: {{ amount | to_vietnamese_words }}
        options.Filters.AddFilter("to_vietnamese_words", (input, arguments, ctx) =>
        {
            var number = (decimal)input.ToNumberValue();
            var unit = arguments.At(0).ToStringValue() ?? "đồng";
            return new StringValue(VietnameseNumberToWordsHelper.ConvertToWords(number, unit));
        });

        // 4. Filter sinh ảnh QR Code Base64: {{ code | qr_code }}
        options.Filters.AddFilter("qr_code", (input, arguments, ctx) =>
        {
            var text = input.ToStringValue();
            return new StringValue(BarcodeQrHelper.GenerateQrCodeBase64(text));
        });

        var context = new TemplateContext(dataModel, options);
        return context;
    }
}
