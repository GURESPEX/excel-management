namespace ExcelManagement.Application.AuditLogs;

public record AuditLogDto(
    int Id,
    string EntityName,
    int EntityId,
    string Action,
    string? ChangedBy,
    DateTime Timestamp,
    string Changes);
