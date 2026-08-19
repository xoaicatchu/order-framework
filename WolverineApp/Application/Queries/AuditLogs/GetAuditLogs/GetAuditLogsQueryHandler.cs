using WolverineApp.Application.Common.Extensions;
using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.DTOs.AuditLogs;
using WolverineApp.Domain.Audit;

namespace WolverineApp.Application.Queries.AuditLogs.GetAuditLogs;

public class GetAuditLogsQueryHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAuditLogsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery query, CancellationToken cancellationToken)
    {
        // 1 dòng gọi duy nhất tự động CountAsync + Skip/Take + Map DTO!
        return await _unitOfWork.GetRepository<AuditLog>().Query()
            .OrderByDescending(l => l.Timestamp)
            .ToPagedResultAsync(query.PageIndex, query.PageSize, l => new AuditLogDto(
                l.Id,
                l.Action,
                l.EntityName,
                l.EntityId,
                l.Details,
                l.Timestamp,
                l.IsSuccess
            ), cancellationToken);
    }
}
