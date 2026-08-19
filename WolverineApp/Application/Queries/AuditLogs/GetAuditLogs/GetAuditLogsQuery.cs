using WolverineApp.Application.Common.Interfaces;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.DTOs.AuditLogs;

namespace WolverineApp.Application.Queries.AuditLogs.GetAuditLogs;

public record GetAuditLogsQuery(
    int PageIndex = 1,
    int PageSize = 10
) : IQuery<PagedResult<AuditLogDto>>;
