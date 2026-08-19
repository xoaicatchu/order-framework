using System.Collections.Concurrent;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Middleware;

public class IdempotencyKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyKeyMiddleware> _logger;
    public const string IdempotencyHeaderName = "Idempotency-Key";

    public record CachedResponse(int StatusCode, string ContentType, string Body);

    private static readonly ConcurrentDictionary<string, CachedResponse> _idempotencyStore = new();

    public IdempotencyKeyMiddleware(RequestDelegate next, ILogger<IdempotencyKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        if (!HttpMethods.IsPost(context.Request.Method) &&
            !HttpMethods.IsPut(context.Request.Method) &&
            !HttpMethods.IsDelete(context.Request.Method))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(IdempotencyHeaderName, out var keyValues) ||
            string.IsNullOrWhiteSpace(keyValues.ToString()))
        {
            await _next(context);
            return;
        }

        var idempotencyKey = keyValues.ToString().Trim();
        var tenantId = tenantProvider.TenantId;
        var cacheKey = $"idempotency:{tenantId}:{idempotencyKey}";

        // 1. Kiểm tra nếu request với Idempotency-Key này đã được xử lý
        if (_idempotencyStore.TryGetValue(cacheKey, out var cachedResponse))
        {
            _logger.LogInformation("⚡ [Idempotency] Duplicate request detected for Key: {Key}. Returning cached response.", idempotencyKey);

            context.Response.StatusCode = cachedResponse.StatusCode;
            context.Response.ContentType = cachedResponse.ContentType;
            context.Response.Headers["X-Idempotency-Hit"] = "true";

            await context.Response.WriteAsync(cachedResponse.Body, Encoding.UTF8);
            return;
        }

        // 2. Nếu là request lần đầu, thực thi và lưu kết quả
        var originalBodyStream = context.Response.Body;
        using var responseBodyMemoryStream = new MemoryStream();
        context.Response.Body = responseBodyMemoryStream;

        try
        {
            await _next(context);

            responseBodyMemoryStream.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(responseBodyMemoryStream).ReadToEndAsync();
            responseBodyMemoryStream.Seek(0, SeekOrigin.Begin);

            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                var responseToCache = new CachedResponse(
                    context.Response.StatusCode,
                    context.Response.ContentType ?? "application/json",
                    responseBodyText);

                _idempotencyStore[cacheKey] = responseToCache;
            }

            await responseBodyMemoryStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}
