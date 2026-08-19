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
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? "default-tenant";
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault() ?? "system";

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
