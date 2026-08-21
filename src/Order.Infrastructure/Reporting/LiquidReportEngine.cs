using System.Text.Json;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Reporting;
using WolverineApp.Infrastructure.Reporting.Helpers;

namespace WolverineApp.Infrastructure.Reporting;

public class LiquidReportEngine : IReportEngine
{
    private static readonly FluidParser Parser = new();
    private const int MaxTemplateLength = 256_000;
    private const int MaxOutputLength = 10_000_000;
    private static readonly MemoryCache TemplateCache = new(new MemoryCacheOptions { SizeLimit = 256 });
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

    public TemplateValidationResult ValidateTemplate(string rawTemplateContent)
    {
        if (string.IsNullOrWhiteSpace(rawTemplateContent))
        {
            return TemplateValidationResult.Error("Nội dung template không được để trống.");
        }

        if (rawTemplateContent.Length > MaxTemplateLength)
        {
            return TemplateValidationResult.Error($"Template vượt quá giới hạn {MaxTemplateLength} ký tự.");
        }

        if (rawTemplateContent.Contains("<script", StringComparison.OrdinalIgnoreCase)
            || rawTemplateContent.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
            || rawTemplateContent.Contains(" onerror=", StringComparison.OrdinalIgnoreCase)
            || rawTemplateContent.Contains(" onclick=", StringComparison.OrdinalIgnoreCase))
        {
            return TemplateValidationResult.Error("Template chứa HTML/JavaScript không được phép.");
        }

        if (!Parser.TryParse(rawTemplateContent, out _, out var error))
        {
            return TemplateValidationResult.Error($"Lỗi cú pháp Liquid: {error}");
        }

        return TemplateValidationResult.Success();
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

        if (rawTemplate.Length > MaxTemplateLength)
        {
            throw new InvalidOperationException("Report template exceeds the configured size limit.");
        }

        IFluidTemplate template;
        if (TemplateCache.TryGetValue(cacheKey, out CachedTemplate? cached) && cached is not null && cached.RawContent == rawTemplate)
        {
            template = cached.Template;
        }
        else
        {
            if (!Parser.TryParse(rawTemplate, out var compiled, out var error))
            {
                throw new InvalidOperationException($"Lỗi biên dịch Liquid template '{templateCode}': {error}");
            }
            TemplateCache.Set(
                cacheKey,
                new CachedTemplate(rawTemplate, compiled),
                new MemoryCacheEntryOptions
                {
                    Size = 1,
                    SlidingExpiration = TimeSpan.FromMinutes(30),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                });
            template = compiled;
        }

        var context = CreateTemplateContext(dataModel);
        var options = context.Options;
        options.MaxSteps = 100_000;
        options.MaxRecursion = 32;
        var renderTask = template.RenderAsync(context).AsTask();
        var completed = await Task.WhenAny(renderTask, Task.Delay(TimeSpan.FromSeconds(5), cancellationToken));
        if (completed != renderTask)
        {
            throw new TimeoutException("Report template rendering exceeded the 5 second limit.");
        }

        var html = await renderTask;
        if (html.Length > MaxOutputLength)
        {
            throw new InvalidOperationException("Rendered report exceeds the configured output size limit.");
        }

        return html;
    }

    public async Task<ReportRenderResult> RenderAsync(ReportRenderRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.TenantId;
        _logger.LogInformation("Rendering report template '{TemplateCode}' in format '{Format}' for tenant '{TenantId}'",
            request.TemplateCode, request.Format, tenantId);

        var compiledHtml = await RenderHtmlAsync(request.TemplateCode, request.DataModel, tenantId, cancellationToken);

        var renderer = _renderers.FirstOrDefault(r => r.SupportedFormat == request.Format);
        if (renderer is null)
        {
            throw new NotSupportedException($"Report output format '{request.Format}' is not supported by any registered renderer.");
        }

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

        // 5. Filter tính tổng (SUM Group/Table): {{ items | sum: 'Total' }}
        options.Filters.AddFilter("sum", (input, arguments, ctx) =>
        {
            var propertyName = arguments.At(0).ToStringValue();
            decimal total = 0;
            var items = input.Enumerate(ctx);
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(propertyName))
                {
                    total += (decimal)item.ToNumberValue();
                }
                else
                {
                    var propVal = item.GetValueAsync(propertyName, ctx).GetAwaiter().GetResult();
                    total += (decimal)propVal.ToNumberValue();
                }
            }
            return NumberValue.Create(total);
        });

        // 6. Filter gom nhóm (GROUP BY): {{ items | group_by: 'Category' }}
        options.Filters.AddFilter("group_by", (input, arguments, ctx) =>
        {
            var propertyName = arguments.At(0).ToStringValue() ?? "Category";
            var resultList = new List<FluidValue>();
            var items = input.Enumerate(ctx);
            var dict = new Dictionary<string, List<object?>>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                var keyVal = item.GetValueAsync(propertyName, ctx).GetAwaiter().GetResult();
                var keyStr = keyVal.ToStringValue() ?? "Khác";
                if (!dict.TryGetValue(keyStr, out var list))
                {
                    list = new List<object?>();
                    dict[keyStr] = list;
                }
                list.Add(item.ToObjectValue());
            }

            foreach (var kvp in dict)
            {
                var groupObj = new Dictionary<string, object>
                {
                    { "Key", kvp.Key },
                    { "Items", kvp.Value },
                    { "Count", kvp.Value.Count }
                };
                resultList.Add(FluidValue.Create(groupObj, options));
            }

            return new ArrayValue(resultList);
        });

        var model = NormalizeDataModel(dataModel);
        var context = new TemplateContext(model, options);
        return context;
    }

    private static object NormalizeDataModel(object dataModel)
    {
        var jsonElement = dataModel is JsonElement element
            ? element
            : JsonSerializer.SerializeToElement(dataModel);

        return ConvertJsonElement(jsonElement) ?? new Dictionary<string, object?>();
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value), StringComparer.OrdinalIgnoreCase),
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetDecimal(out var d) ? d : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    private sealed record CachedTemplate(string RawContent, IFluidTemplate Template);
}
