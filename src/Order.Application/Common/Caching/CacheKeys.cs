namespace WolverineApp.Application.Common.Caching;

public static class CacheKeys
{
    public static string Order(string tenantId, Guid orderId) => $"tenant:{tenantId}:order:{orderId}";
    public static string OrderTag(string tenantId) => $"tenant:{tenantId}:orders";
    public static string Statistics(string tenantId) => $"tenant:{tenantId}:statistics";
}
