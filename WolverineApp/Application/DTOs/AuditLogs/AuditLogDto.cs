namespace WolverineApp.Application.DTOs.AuditLogs;

public record AuditLogDto(
    Guid Id,
    string Action,
    string EntityName,
    string EntityId,
    string Details,
    DateTime Timestamp,
    bool IsSuccess
);
