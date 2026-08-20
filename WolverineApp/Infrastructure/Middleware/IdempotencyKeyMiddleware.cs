using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Infrastructure.Persistence.Models;

namespace WolverineApp.Infrastructure.Middleware;

public class IdempotencyKeyMiddleware
{
    private const int MaxCachedResponseBytes = 2 * 1024 * 1024;
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyKeyMiddleware> _logger;
    public const string IdempotencyHeaderName = "Idempotency-Key";

    public IdempotencyKeyMiddleware(RequestDelegate next, ILogger<IdempotencyKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ICurrentUserProvider currentUserProvider)
    {
        if (!HttpMethods.IsPost(context.Request.Method)
            && !HttpMethods.IsPut(context.Request.Method)
            && !HttpMethods.IsDelete(context.Request.Method))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true
            || !context.Request.Headers.TryGetValue(IdempotencyHeaderName, out var keyValues))
        {
            await _next(context);
            return;
        }

        var idempotencyKey = keyValues.ToString().Trim();
        if (idempotencyKey.Length is 0 or > 200)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Idempotency-Key must be between 1 and 200 characters." });
            return;
        }

        context.Request.EnableBuffering();
        using var requestBody = new MemoryStream();
        await context.Request.Body.CopyToAsync(requestBody, context.RequestAborted);
        context.Request.Body.Position = 0;
        var requestHash = Convert.ToHexString(SHA256.HashData(requestBody.ToArray()));

        var tenantId = tenantProvider.TenantId;
        var userId = currentUserProvider.UserId;
        var now = DateTime.UtcNow;
        var idempotencyRepository = unitOfWork.GetRepository<HttpIdempotencyRecord>();
        var existing = await idempotencyRepository.Query(tracking: true)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId
                                      && r.UserId == userId
                                      && r.Method == context.Request.Method
                                      && r.Path == context.Request.Path.Value
                                      && r.IdempotencyKey == idempotencyKey,
                context.RequestAborted);

        if (existing is not null)
        {
            if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new { error = "The idempotency key was already used with a different request." });
                return;
            }

            if (existing.Status == "Completed" && existing.ExpiresAtUtc > now && existing.ResponseBody is not null)
            {
                context.Response.StatusCode = existing.ResponseStatusCode ?? StatusCodes.Status200OK;
                context.Response.ContentType = existing.ResponseContentType ?? "application/json";
                context.Response.Headers["X-Idempotency-Hit"] = "true";
                await context.Response.WriteAsync(existing.ResponseBody, Encoding.UTF8, context.RequestAborted);
                return;
            }

            if (existing.Status == "Processing" && existing.ExpiresAtUtc > now)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new { error = "A request with this idempotency key is already processing." });
                return;
            }

            existing.Status = "Processing";
            existing.ExpiresAtUtc = now.AddHours(24);
            existing.ResponseBody = null;
        }
        else
        {
            existing = new HttpIdempotencyRecord
            {
                TenantId = tenantId,
                UserId = userId,
                Method = context.Request.Method,
                Path = context.Request.Path.Value ?? string.Empty,
                IdempotencyKey = idempotencyKey,
                RequestHash = requestHash,
                ExpiresAtUtc = now.AddHours(24)
            };
            await idempotencyRepository.AddAsync(existing, context.RequestAborted);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(context.RequestAborted);
        }
        catch (DbUpdateException)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new { error = "A request with this idempotency key is already processing." });
            return;
        }

        var originalBodyStream = context.Response.Body;
        await using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
            await FinalizeRecordAsync(context, unitOfWork, existing, responseBodyStream);
            responseBodyStream.Position = 0;
            await responseBodyStream.CopyToAsync(originalBodyStream, context.RequestAborted);
        }
        catch
        {
            // Do not leave a failed request permanently stuck in Processing.
            existing.Status = "Failed";
            existing.ResponseStatusCode = null;
            existing.ResponseContentType = null;
            existing.ResponseBody = null;
            existing.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5);
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task FinalizeRecordAsync(
        HttpContext context,
        IUnitOfWork unitOfWork,
        HttpIdempotencyRecord record,
        MemoryStream responseBodyStream)
    {
        responseBodyStream.Position = 0;
        var responseBytes = responseBodyStream.ToArray();
        var cacheable = context.Response.StatusCode is >= 200 and < 300
                        && responseBytes.Length <= MaxCachedResponseBytes
                        && (context.Response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true);

        record.Status = cacheable ? "Completed" : "Failed";
        record.ResponseStatusCode = cacheable ? context.Response.StatusCode : null;
        record.ResponseContentType = cacheable ? context.Response.ContentType : null;
        record.ResponseBody = cacheable ? Encoding.UTF8.GetString(responseBytes) : null;
        record.ExpiresAtUtc = DateTime.UtcNow.AddHours(24);

        await unitOfWork.SaveChangesAsync(context.RequestAborted);
        _logger.LogDebug("Finalized idempotency record {RecordId} with status {Status}", record.Id, record.Status);
    }
}
