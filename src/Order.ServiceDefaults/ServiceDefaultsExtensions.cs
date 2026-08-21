using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WolverineApp.ServiceDefaults;

public static class ServiceDefaultsExtensions
{
    public static IServiceCollection AddOrderServiceDefaults(this IServiceCollection services)
    {
        services.AddProblemDetails();
        return services;
    }

    public static IApplicationBuilder UseOrderServiceDefaults(this IApplicationBuilder app)
    {
        return app.UseStatusCodePages();
    }
}
