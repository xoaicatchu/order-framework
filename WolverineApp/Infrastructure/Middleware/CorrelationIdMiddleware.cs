using Serilog.Context;

namespace WolverineApp.Infrastructure.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[CorrelationIdHeaderName] = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        var clientIp = context.Connection.RemoteIpAddress?.ToString()
                       ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                       ?? "unknown";

        var userAgent = context.Request.Headers.UserAgent.ToString();
        var tenantId = context.User.FindFirst("tenant_id")?.Value
                       ?? context.User.FindFirst("tenant")?.Value
                       ?? "anonymous";
        var userId = context.User.FindFirst("sub")?.Value
                     ?? context.User.Identity?.Name
                     ?? "anonymous";

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("ClientIp", clientIp))
        using (LogContext.PushProperty("UserAgent", userAgent))
        using (LogContext.PushProperty("TenantId", tenantId))
        using (LogContext.PushProperty("UserId", userId))
        {
            await _next(context);
        }
    }
}
