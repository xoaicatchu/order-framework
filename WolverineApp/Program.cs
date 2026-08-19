using System.Text;
using System.Threading.RateLimiting;
using JasperFx.CodeGeneration.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Wolverine;
using Wolverine.FluentValidation;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Domain.Common;
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

// 3. Security & IAM: JWT Bearer Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "ThisIsASecretKeyForJwtAuthenticationInEnterpriseSystem123456!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EnterpriseDistributedCore";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EnterpriseDistributedCoreClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// 4. Security & IAM: RBAC Policy-Based Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Permissions.Orders.Read, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(Roles.Admin) ||
            ctx.User.IsInRole(Roles.Manager) ||
            ctx.User.IsInRole(Roles.Operator) ||
            ctx.User.IsInRole(Roles.Viewer) ||
            ctx.User.HasClaim("permission", Permissions.Orders.Read)));

    options.AddPolicy(Permissions.Orders.Create, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(Roles.Admin) ||
            ctx.User.IsInRole(Roles.Manager) ||
            ctx.User.IsInRole(Roles.Operator) ||
            ctx.User.HasClaim("permission", Permissions.Orders.Create)));

    options.AddPolicy(Permissions.Orders.Update, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(Roles.Admin) ||
            ctx.User.IsInRole(Roles.Manager) ||
            ctx.User.IsInRole(Roles.Operator) ||
            ctx.User.HasClaim("permission", Permissions.Orders.Update)));

    options.AddPolicy(Permissions.Orders.Cancel, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(Roles.Admin) ||
            ctx.User.IsInRole(Roles.Manager) ||
            ctx.User.HasClaim("permission", Permissions.Orders.Cancel)));

    options.AddPolicy(Permissions.AuditLogs.Read, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(Roles.Admin) ||
            ctx.User.HasClaim("permission", Permissions.AuditLogs.Read)));
});

builder.Services.AddSingleton<IAuthTokenService, AuthTokenService>();

// 5. Rate Limiting (Chống DDoS & Brute-force)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// 6. Controllers, Swagger with JWT Definition & Mapster
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Enterprise Distributed Application Platform API", Version = "v1" });
    
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Nhập token JWT theo định dạng: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

Mapster.TypeAdapterConfig.GlobalSettings.Scan(System.Reflection.Assembly.GetExecutingAssembly());

// 7. Telemetry & Context Providers
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

// 8. Persistence, Unit Of Work, Repository & Idempotency
builder.Services.AddScoped<AuditableEntityInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db";
    options.UseSqlite(connStr)
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
           .AddInterceptors(interceptor);
});

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();

// 9. Enterprise Hybrid Caching (L1 In-Memory + L2 Redis Distributed Cache)
#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});
#pragma warning restore EXTEXP0018

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "EDAP_";
    });
}

builder.Services.AddSingleton<ICacheService, HybridCacheService>();

// 10. Transactional Outbox Background Processor (Reliable Event Dispatcher)
builder.Services.AddHostedService<OutboxBackgroundProcessor>();

// 11. Enterprise Health Checks (Liveness, Readiness & System Telemetry)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(name: "database", tags: ["ready"])
    .AddCheck<SystemMemoryHealthCheck>(name: "memory", tags: ["live", "ready"]);

// 12. Wolverine Message Bus & CQRS Pipeline
builder.Host.UseWolverine(opts =>
{
    opts.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
    opts.UseFluentValidation();
    // Tự động kích hoạt AOP Middleware xóa cache sau khi bất kỳ Command nào hoàn tất
    opts.Policies.AddMiddleware(typeof(CacheInvalidationMiddleware));
});

var app = builder.Build();

// 13. Auto-Migration on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    Log.Information("Database schema initialized and verified successfully");
}

// 14. HTTP Telemetry & Security Pipeline
app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRateLimiter();

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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Enterprise Platform API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// Kích hoạt Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Kích hoạt Idempotency-Key Middleware (Sau Auth để trích xuất Tenant an toàn)
app.UseMiddleware<IdempotencyKeyMiddleware>();

// 15. Map Health Check Endpoints (K8s Liveness / Readiness Probes)
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
