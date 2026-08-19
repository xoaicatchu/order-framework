using JasperFx.CodeGeneration.Model;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Wolverine;
using Wolverine.FluentValidation;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Infrastructure.BackgroundServices;
using WolverineApp.Infrastructure.Data;
using WolverineApp.Infrastructure.Data.Interceptors;
using WolverineApp.Infrastructure.Data.Repositories;
using WolverineApp.Infrastructure.Health;
using WolverineApp.Infrastructure.Middleware;
using WolverineApp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Enterprise Structured Logging (Serilog từ appsettings.json)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Cấu hình Forwarded Headers cho Kubernetes Ingress / API Gateway (Kong, Nginx, ALB)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// 3. Controllers & Swagger API Documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Wolverine Enterprise Order Management API", Version = "v1" });
});

// 4. Telemetry & Context Providers
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

// 5. Persistence, Unit Of Work, Repository & Idempotency
builder.Services.AddScoped<AuditableEntityInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
    options.UseSqlite("Data Source=orders.db")
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
           .AddInterceptors(interceptor);
});

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();

// 6. Transactional Outbox Background Processor (Reliable Event Dispatcher)
builder.Services.AddHostedService<OutboxBackgroundProcessor>();

// 7. Enterprise Health Checks (Liveness, Readiness & System Telemetry)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(name: "database", tags: ["ready"])
    .AddCheck<SystemMemoryHealthCheck>(name: "memory", tags: ["live", "ready"]);

// 8. Wolverine Message Bus & CQRS Pipeline
builder.Host.UseWolverine(opts =>
{
    opts.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
    opts.UseFluentValidation();
});

var app = builder.Build();

// 9. Auto-Migration on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    Log.Information("Database schema initialized and verified successfully");
}

// 10. HTTP Telemetry & Middleware Pipeline (Hỗ trợ K8s Reverse Proxy)
app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("TenantId", httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? "default-tenant");
        diagnosticContext.Set("UserId", httpContext.Request.Headers["X-User-Id"].FirstOrDefault() ?? "system");
        if (httpContext.Items.TryGetValue("X-Correlation-Id", out var corrId) && corrId is not null)
        {
            diagnosticContext.Set("CorrelationId", corrId);
        }
    };
});

app.UseMiddleware<ValidationExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();

// 11. Map Health Check Endpoints (K8s Liveness / Readiness Probes)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapControllers();

app.Run();
