namespace WolverineApp.Application.Common.Interfaces;

public interface ITenantProvider
{
    string TenantId { get; }
    bool IsHttpRequest { get; }
}
