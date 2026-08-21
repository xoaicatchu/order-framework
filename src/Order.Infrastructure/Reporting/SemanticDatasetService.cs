using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Reporting;
using WolverineApp.Domain.Reporting;

namespace WolverineApp.Infrastructure.Reporting;

public class SemanticDatasetService : ISemanticDatasetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SemanticDatasetService> _logger;

    public SemanticDatasetService(
        IUnitOfWork unitOfWork,
        ILogger<SemanticDatasetService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<SemanticDatasetDto>> GetAvailableDatasetsAsync(CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<SemanticDataset>();
        var datasets = await repository.Query()
            .Where(d => d.IsActive && !d.IsDeleted)
            .OrderBy(d => d.Category)
            .ThenBy(d => d.Name)
            .ToListAsync(cancellationToken);

        var result = new List<SemanticDatasetDto>();
        foreach (var ds in datasets)
        {
            var fields = JsonSerializer.Deserialize<List<SemanticFieldDefinition>>(ds.FieldsMetadataJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            result.Add(new SemanticDatasetDto(ds.Code, ds.Name, ds.Category, ds.Description, fields));
        }

        return result;
    }

    public async Task<SemanticDatasetDto?> GetDatasetByCodeAsync(string datasetCode, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<SemanticDataset>();
        var ds = await repository.Query()
            .FirstOrDefaultAsync(d => d.Code == datasetCode && d.IsActive && !d.IsDeleted, cancellationToken);

        if (ds is null) return null;

        var fields = JsonSerializer.Deserialize<List<SemanticFieldDefinition>>(ds.FieldsMetadataJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        return new SemanticDatasetDto(ds.Code, ds.Name, ds.Category, ds.Description, fields);
    }

    public async Task<List<Dictionary<string, object?>>> ExecuteDatasetQueryAsync(
        string datasetCode,
        string tenantId,
        List<string> selectedFields,
        Dictionary<string, object?> filterCriteria,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<SemanticDataset>();
        var dataset = await repository.Query()
            .FirstOrDefaultAsync(d => d.Code == datasetCode && d.IsActive && !d.IsDeleted, cancellationToken);

        if (dataset is null)
        {
            throw new ArgumentException($"Semantic Dataset '{datasetCode}' không tồn tại trên hệ thống.");
        }

        var validFields = JsonSerializer.Deserialize<List<SemanticFieldDefinition>>(dataset.FieldsMetadataJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        var fieldDict = validFields
            .Where(f => IsSafeIdentifier(f.Key))
            .ToDictionary(f => f.Key, f => f, StringComparer.OrdinalIgnoreCase);

        if (!dataset.BaseQuerySql.Contains("@TenantId", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Dataset '{datasetCode}' does not declare the required @TenantId predicate.");
        }

        var requestedFields = selectedFields
            .Where(fieldDict.ContainsKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (requestedFields.Count == 0)
        {
            requestedFields = fieldDict.Keys.ToList();
        }

        // CreatedAt is an internal ordering column. It is never returned unless the
        // caller explicitly selected it, but it must be present in the subquery.
        var projectedFields = requestedFields.Contains("CreatedAt", StringComparer.OrdinalIgnoreCase)
            ? requestedFields
            : [.. requestedFields, "CreatedAt"];

        var queryBuilder = new StringBuilder();
        queryBuilder.AppendLine($"SELECT {string.Join(", ", projectedFields.Select(QuoteIdentifier))} FROM (");
        queryBuilder.AppendLine(dataset.BaseQuerySql);
        queryBuilder.AppendLine(") AS BaseDataset");
        queryBuilder.AppendLine("WHERE 1=1");

        var connection = _unitOfWork.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        using var command = connection.CreateCommand();
        command.CommandTimeout = 15;

        // Force @TenantId
        AddParameter(command, "@TenantId", tenantId);

        // Map friendly filter criteria to parameterized WHERE clauses.
        // Examples: { "CreatedAt": { "from": "2026-08-01", "to": "2026-08-31" } }
        // becomes CreatedAt_from / CreatedAt_to after the public API normalizes it.
        foreach (var (key, value) in filterCriteria)
        {
            if (value is null || string.IsNullOrWhiteSpace(value.ToString())) continue;

            var normalizedKey = key.Replace('.', '_');
            var isFrom = normalizedKey.EndsWith("_from", StringComparison.OrdinalIgnoreCase);
            var isTo = normalizedKey.EndsWith("_to", StringComparison.OrdinalIgnoreCase);
            var fieldName = isFrom || isTo
                ? normalizedKey[..normalizedKey.LastIndexOf('_')]
                : normalizedKey;

            // Backward-compatible aliases used by older integrations.
            if (key.Equals("FromDate", StringComparison.OrdinalIgnoreCase)
                || key.Equals("ToDate", StringComparison.OrdinalIgnoreCase))
            {
                fieldName = "CreatedAt";
                isFrom = key.Equals("FromDate", StringComparison.OrdinalIgnoreCase);
                isTo = !isFrom;
            }

            if (fieldDict.TryGetValue(fieldName, out var fieldDef) && fieldDef.Filterable)
            {
                if ((isFrom || isTo) && fieldDef.Type == "date"
                    && DateTime.TryParse(value.ToString(), out var dateValue))
                {
                    var parameterName = $"@Param_{fieldDef.Key}_{(isFrom ? "From" : "To")}";
                    queryBuilder.AppendLine($"  AND {QuoteIdentifier(fieldDef.Key)} {(isFrom ? ">=" : "<=" )} {parameterName}");
                    AddParameter(command, parameterName, isFrom ? dateValue.Date : dateValue.Date.AddDays(1).AddTicks(-1));
                    continue;
                }

                var paramName = $"@Param_{fieldDef.Key}";
                if (fieldDef.Type == "string")
                {
                    queryBuilder.AppendLine($"  AND {QuoteIdentifier(fieldDef.Key)} LIKE {paramName}");
                    AddParameter(command, paramName, $"%{value}%");
                }
                else
                {
                    queryBuilder.AppendLine($"  AND {QuoteIdentifier(fieldDef.Key)} = {paramName}");
                    AddParameter(command, paramName, value);
                }
            }
        }

        queryBuilder.AppendLine($"ORDER BY {QuoteIdentifier("CreatedAt")} DESC");
        queryBuilder.AppendLine("LIMIT 10000");

        command.CommandText = queryBuilder.ToString();
        _logger.LogInformation("Executing Semantic Dataset query '{DatasetCode}' for tenant '{TenantId}'", datasetCode, tenantId);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var resultList = new List<Dictionary<string, object?>>();

        var columnNames = new List<string>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            columnNames.Add(reader.GetName(i));
        }

        while (await reader.ReadAsync(cancellationToken))
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            if (selectedFields.Count > 0)
            {
                foreach (var field in requestedFields)
                {
                    var colIdx = columnNames.FindIndex(c => c.Equals(field, StringComparison.OrdinalIgnoreCase));
                    if (colIdx >= 0 && !reader.IsDBNull(colIdx))
                    {
                        dict[field] = reader.GetValue(colIdx);
                    }
                }
            }
            else
            {
                for (int i = 0; i < columnNames.Count; i++)
                {
                    dict[columnNames[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
            }

            resultList.Add(dict);
        }

        return resultList;
    }

    public string GenerateDefaultLiquidTemplate(string reportName, List<SemanticFieldDefinition> selectedFields)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"vi\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <style>");
        sb.AppendLine("        @page { size: A4 portrait; margin: 12mm 15mm; }");
        sb.AppendLine("        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif; font-size: 12px; color: #2c3e50; margin: 0; padding: 15px; }");
        sb.AppendLine("        .header { display: flex; justify-content: space-between; border-bottom: 2px solid #2563eb; padding-bottom: 10px; margin-bottom: 15px; }");
        sb.AppendLine("        .title { text-align: center; margin: 15px 0; }");
        sb.AppendLine("        .title h1 { margin: 0; color: #1e3a8a; font-size: 18px; text-transform: uppercase; }");
        sb.AppendLine("        .meta { font-size: 11px; color: #64748b; font-style: italic; margin-top: 4px; }");
        sb.AppendLine("        table { width: 100%; border-collapse: collapse; margin-top: 15px; }");
        sb.AppendLine("        th, td { border: 1px solid #cbd5e1; padding: 6px 8px; font-size: 11px; }");
        sb.AppendLine("        th { background-color: #f1f5f9; font-weight: bold; color: #1e293b; text-align: center; }");
        sb.AppendLine("        tr:nth-child(even) { background-color: #f8fafc; }");
        sb.AppendLine("        .text-right { text-align: right; }");
        sb.AppendLine("        .text-center { text-align: center; }");
        sb.AppendLine("        .footer-summary { margin-top: 15px; display: flex; justify-content: space-between; font-weight: bold; font-size: 13px; color: #1e3a8a; }");
        sb.AppendLine("        .signature-grid { display: flex; justify-content: space-around; margin-top: 30px; text-align: center; font-size: 12px; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // Header
        sb.AppendLine("    <div class=\"header\">");
        sb.AppendLine("        <div><strong>HỆ THỐNG Y TẾ QUẢN TRỊ DOANH NGHIỆP PHÂN TÁN</strong><br/><small>Phòng Kế Hoạch Tổng Hợp & Tài Chính</small></div>");
        sb.AppendLine("        <div style=\"text-align: right;\"><small>Mẫu báo cáo chuẩn Enterprise</small><br/><small>Ngày in: {{ 'now' | format_date: 'dd/MM/yyyy HH:mm' }}</small></div>");
        sb.AppendLine("    </div>");

        // Title
        sb.AppendLine("    <div class=\"title\">");
        sb.AppendLine($"        <h1>{reportName}</h1>");
        sb.AppendLine("        <div class=\"meta\">Tổng số bản ghi: {{ Data.size }} | Người lập: Ban Điều Hành</div>");
        sb.AppendLine("    </div>");

        // Table
        sb.AppendLine("    <table>");
        sb.AppendLine("        <thead>");
        sb.AppendLine("            <tr>");
        sb.AppendLine("                <th style=\"width: 35px;\">STT</th>");

        foreach (var field in selectedFields)
        {
            var alignClass = (field.Type == "currency" || field.Type == "number") ? " class=\"text-right\"" : "";
            sb.AppendLine($"                <th{alignClass}>{field.Label}</th>");
        }

        sb.AppendLine("            </tr>");
        sb.AppendLine("        </thead>");
        sb.AppendLine("        <tbody>");
        sb.AppendLine("            {% for row in Data %}");
        sb.AppendLine("            <tr>");
        sb.AppendLine("                <td class=\"text-center\">{{ forloop.index }}</td>");

        foreach (var field in selectedFields)
        {
            if (field.Type == "currency")
            {
                sb.AppendLine($"                <td class=\"text-right\">{{{{ row.{field.Key} | format_currency: 'VND' }}}}</td>");
            }
            else if (field.Type == "date")
            {
                sb.AppendLine($"                <td class=\"text-center\">{{{{ row.{field.Key} | format_date: 'dd/MM/yyyy' }}}}</td>");
            }
            else
            {
                sb.AppendLine($"                <td>{{{{ row.{field.Key} }}}}</td>");
            }
        }

        sb.AppendLine("            </tr>");
        sb.AppendLine("            {% endfor %}");
        sb.AppendLine("        </tbody>");
        sb.AppendLine("    </table>");

        // Summary box
        sb.AppendLine("    <div class=\"footer-summary\">");
        sb.AppendLine("        <div>Tổng cộng: {{ Data.size }} dòng dữ liệu</div>");
        sb.AppendLine("    </div>");

        // Signatures
        sb.AppendLine("    <div class=\"signature-grid\">");
        sb.AppendLine("        <div><strong>NGƯỜI LẬP BIỂU</strong><br/><small>(Ký và ghi rõ họ tên)</small></div>");
        sb.AppendLine("        <div><strong>TRƯỞNG PHÒNG TÀI CHÍNH</strong><br/><small>(Ký và ghi rõ họ tên)</small></div>");
        sb.AppendLine("        <div><strong>GIÁM ĐỐC ĐƠN VỊ</strong><br/><small>(Ký tên và đóng dấu)</small></div>");
        sb.AppendLine("    </div>");

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        command.Parameters.Add(param);
    }

    private static bool IsSafeIdentifier(string value)
        => Regex.IsMatch(value, "^[A-Za-z_][A-Za-z0-9_]*$");

    private static string QuoteIdentifier(string value)
        => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
