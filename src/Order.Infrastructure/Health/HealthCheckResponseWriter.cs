using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WolverineApp.Infrastructure.Health;

public static class HealthCheckResponseWriter
{
    public static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new { status = report.Status.ToString() };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        return context.Response.WriteAsync(json);
    }
}
