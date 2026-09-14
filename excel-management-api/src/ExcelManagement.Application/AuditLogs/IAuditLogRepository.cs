namespace ExcelManagement.Application.AuditLogs;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLogDto>> GetAsync(AuditLogQuery query, CancellationToken ct);
}
