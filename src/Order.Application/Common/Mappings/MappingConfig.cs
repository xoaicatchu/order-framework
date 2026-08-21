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
        // -------------------------------------------------------------
        // 1. INBOUND MAPPINGS (DTOs / Inputs -> Domain Entities / Value Objects)
        // -------------------------------------------------------------
        config.NewConfig<CreateOrderItemDto, OrderItem>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Total, src => src.Quantity * src.UnitPrice);

        // -------------------------------------------------------------
        // 2. OUTBOUND MAPPINGS (Domain Entities -> DTOs / Response Models)
        // -------------------------------------------------------------
        config.NewConfig<Order, OrderDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Items, src => src.Items);

        config.NewConfig<OrderItem, OrderItemDto>();

        config.NewConfig<AuditLog, AuditLogDto>();
    }
}
