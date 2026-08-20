using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WolverineApp.Domain.Identity;
using WolverineApp.Infrastructure.Data;

namespace WolverineApp.Infrastructure.Auth;

public static class PermissionDiscoveryService
{
    public const string RootPermissionCode = "System:Root";

    public static async Task DiscoverAndSyncPermissionsAsync(ApplicationDbContext dbContext, Assembly assembly, ILogger logger)
    {
        var discoveredPermissions = new List<AppPermission>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Code = RootPermissionCode,
                Module = "System",
                Resource = "System",
                Action = "Root",
                IsAutoDiscovered = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var controllerTypes = assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var controller in controllerTypes)
        {
            var resourceAttr = controller.GetCustomAttribute<PermissionResourceAttribute>();
            var defaultResource = resourceAttr?.Resource ?? controller.Name.Replace("Controller", "");
            var defaultModule = resourceAttr?.Module ?? "General";

            var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var permAttr = method.GetCustomAttribute<HasPermissionAttribute>();
                if (permAttr is not null)
                {
                    var resource = !string.IsNullOrWhiteSpace(permAttr.Resource) ? permAttr.Resource : defaultResource;
                    var action = permAttr.Action;
                    var code = $"{resource}:{action}";

                    if (!discoveredPermissions.Any(p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase)))
                    {
                        discoveredPermissions.Add(new AppPermission
                        {
                            Id = Guid.NewGuid(),
                            Code = code,
                            Module = defaultModule,
                            Resource = resource,
                            Action = action,
                            IsAutoDiscovered = true,
                            IsSystem = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        var existingCodes = await dbContext.Permissions
            .Select(p => p.Code)
            .ToListAsync();

        var newPermissions = discoveredPermissions
            .Where(dp => !existingCodes.Contains(dp.Code, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (newPermissions.Count > 0)
        {
            await dbContext.Permissions.AddRangeAsync(newPermissions);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Synced {Count} new permissions into database", newPermissions.Count);
        }
    }
}
