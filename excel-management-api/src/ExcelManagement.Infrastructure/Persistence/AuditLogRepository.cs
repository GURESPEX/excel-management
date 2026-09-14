using ExcelManagement.Application.AuditLogs;
using Microsoft.EntityFrameworkCore;

namespace ExcelManagement.Infrastructure.Persistence;

public class AuditLogRepository(AppDbContext db) : IAuditLogRepository
{
    public async Task<IReadOnlyList<AuditLogDto>> GetAsync(AuditLogQuery filter, CancellationToken ct)
    {
        var query = db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.EntityName))
        {
            query = query.Where(a => a.EntityName == filter.EntityName);
        }

        if (filter.From is { } from)
        {
            query = query.Where(a => a.Timestamp >= from);
        }

        if (filter.To is { } to)
        {
            query = query.Where(a => a.Timestamp <= to);
        }

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new AuditLogDto(a.Id, a.EntityName, a.EntityId, a.Action.ToString(), a.ChangedBy, a.Timestamp, a.Changes))
            .ToListAsync(ct);
    }
}
