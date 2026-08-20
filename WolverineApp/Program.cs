using System.Text;
using System.Threading.RateLimiting;
using JasperFx.CodeGeneration.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
using WolverineApp.Domain.Identity;
using WolverineApp.Infrastructure.Auth;
using WolverineApp.Infrastructure.BackgroundServices;
using WolverineApp.Infrastructure.Data;
using WolverineApp.Infrastructure.Data.Interceptors;
using WolverineApp.Infrastructure.Data.Repositories;
using WolverineApp.Infrastructure.Health;
using WolverineApp.Infrastructure.Middleware;
using WolverineApp.Infrastructure.Services;
using WolverineApp.Application.Common.Reporting;
using WolverineApp.Infrastructure.Reporting;
using WolverineApp.Infrastructure.Reporting.TemplateStores;
using WolverineApp.Infrastructure.Reporting.Renderers;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

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

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthTokenService, AuthTokenService>();

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Enterprise Distributed Application Platform API", Version = "v1" });
    
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token: Bearer {token}",
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

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
builder.Services.AddHostedService<OutboxBackgroundProcessor>();

// Enterprise Template-Driven Reporting & Document Rendering Engine (Database-Backed Template Store)
builder.Services.AddSingleton<IReportTemplateStore, WolverineApp.Infrastructure.Reporting.TemplateStores.DbReportTemplateStore>();
builder.Services.AddSingleton<WolverineApp.Application.Common.Reporting.IDocumentRenderer, WolverineApp.Infrastructure.Reporting.Renderers.QuestPdfDocumentRenderer>();
builder.Services.AddSingleton<WolverineApp.Application.Common.Reporting.IDocumentRenderer, WolverineApp.Infrastructure.Reporting.Renderers.HtmlDocumentRenderer>();
builder.Services.AddScoped<WolverineApp.Application.Common.Reporting.IReportEngine, WolverineApp.Infrastructure.Reporting.LiquidReportEngine>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(name: "database", tags: ["ready"])
    .AddCheck<SystemMemoryHealthCheck>(name: "memory", tags: ["live", "ready"]);

builder.Host.UseWolverine(opts =>
{
    opts.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
    opts.UseFluentValidation();
    opts.Policies.AddMiddleware(typeof(CacheInvalidationMiddleware));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    await DbInitializer.SeedInitialDataAsync(db, app.Logger);
    Log.Information("Database schema initialized and dynamic permissions synchronized");
}

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
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<IdempotencyKeyMiddleware>();

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
