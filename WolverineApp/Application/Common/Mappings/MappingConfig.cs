using Mapster;
using WolverineApp.Application.DTOs.AuditLogs;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Domain.Audit;
using WolverineApp.Domain.Orders;

namespace WolverineApp.Application.Common.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Order, OrderDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Items, src => src.Items);

        config.NewConfig<OrderItem, OrderItemDto>();

        config.NewConfig<AuditLog, AuditLogDto>();
    }
}
