namespace ExcelManagement.Application.AuditLogs;

public record AuditLogQuery(
    string? EntityName = null,
    DateTime? From = null,
    DateTime? To = null);
