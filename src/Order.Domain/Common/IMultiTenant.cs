namespace WolverineApp.Domain.Common;

public interface IMultiTenant
{
    string TenantId { get; set; }
}
