using System.Security.Claims;
using System.Text.Json;
using ExcelManagement.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ExcelManagement.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Sqlite and Postgres intentionally end up with different column types for
        // the same entities (see the PostgresInitialCreate migration) — EF Core's
        // "pending model changes" runtime check would otherwise treat that expected
        // divergence as a forgotten migration whenever the *other* provider is active.
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Salary).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Department>(d =>
        {
            d.Property(x => x.Name).IsRequired().HasMaxLength(100);
            d.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<User>(u =>
        {
            u.Property(x => x.Username).IsRequired().HasMaxLength(100);
            u.Property(x => x.PasswordHash).IsRequired();
            u.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(a =>
        {
            a.Property(x => x.EntityName).IsRequired().HasMaxLength(100);
            a.Property(x => x.ChangedBy).HasMaxLength(100);
            a.Property(x => x.Changes).IsRequired();
            a.HasIndex(x => x.EntityName);
            a.HasIndex(x => x.Timestamp);
        });
    }

    // Single shared audit-writing mechanism: every Employee/Department create,
    // update or delete gets one AuditLog row, written in the same operation as
    // the triggering change — so no endpoint/use-case has to remember to log
    // anything itself.
    //
    // This runs in two passes rather than the more obvious "add the AuditLog
    // rows to the same SaveChanges call" because EntityId for a *new* row isn't
    // known until the database assigns it — Employee/Department use DB-generated
    // identity keys, and AuditLog.EntityId is a plain int copy, not a real FK
    // EF can defer for us. So: capture the diff now (OriginalValue/CurrentValue
    // are only meaningful pre-save), save the real change to generate any new
    // ids, then save the audit rows — wrapped in one transaction so the pair is
    // still atomic.
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var pending = CaptureAuditEntries();
        if (pending.Count == 0)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        var changedBy = GetChangedBy();
        var timestamp = DateTime.UtcNow;

        var ownsTransaction = Database.CurrentTransaction is null;
        var transaction = ownsTransaction ? await Database.BeginTransactionAsync(cancellationToken) : null;
        try
        {
            var affected = await base.SaveChangesAsync(cancellationToken);
            foreach (var entry in pending)
            {
                AuditLogs.Add(entry.ToAuditLog(changedBy, timestamp));
            }

            affected += await base.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return affected;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public override int SaveChanges()
    {
        var pending = CaptureAuditEntries();
        if (pending.Count == 0)
        {
            return base.SaveChanges();
        }

        var changedBy = GetChangedBy();
        var timestamp = DateTime.UtcNow;

        var ownsTransaction = Database.CurrentTransaction is null;
        var transaction = ownsTransaction ? Database.BeginTransaction() : null;
        try
        {
            var affected = base.SaveChanges();
            foreach (var entry in pending)
            {
                AuditLogs.Add(entry.ToAuditLog(changedBy, timestamp));
            }

            affected += base.SaveChanges();
            transaction?.Commit();
            return affected;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    private string? GetChangedBy()
    {
        // No HttpContext outside a request (e.g. startup seeding, or a test
        // that writes straight to the DbContext) — record no changed-by rather
        // than throwing.
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
    }

    private List<PendingAuditEntry> CaptureAuditEntries()
    {
        var result = new List<PendingAuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not (Employee or Department))
            {
                continue;
            }

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Deleted => AuditAction.Deleted,
                _ => AuditAction.Updated,
            };

            // Created rows: Id (and any FK pointing at another new-in-this-batch
            // row, e.g. seeding an Employee alongside its Department) are only
            // EF's temporary placeholder values until the insert round-trips —
            // both the diff and the entity id must be read *after* the first
            // SaveChanges below, not now. Updated/Deleted rows use existing,
            // already-resolved keys, so both can be captured immediately —
            // Updated in particular needs Original/CurrentValue captured now,
            // since SaveChanges resets OriginalValue to match CurrentValue.
            string? changes = null;
            int? entityId = null;
            if (action != AuditAction.Created)
            {
                changes = BuildChangesJson(entry, action);
                if (action == AuditAction.Updated && changes is null)
                {
                    // Entity was touched (e.g. a related nav property) but no
                    // scalar column actually changed — nothing worth auditing.
                    continue;
                }

                entityId = (int)entry.Property("Id").CurrentValue!;
            }

            result.Add(new PendingAuditEntry(entry, entry.Entity.GetType().Name, entityId, action, changes));
        }

        return result;
    }

    private static string? BuildChangesJson(EntityEntry entry, AuditAction action)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            var name = property.Metadata.Name;
            switch (action)
            {
                case AuditAction.Created:
                    changes[name] = property.CurrentValue;
                    break;
                case AuditAction.Deleted:
                    changes[name] = property.OriginalValue;
                    break;
                case AuditAction.Updated:
                    if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                    {
                        changes[name] = new { from = property.OriginalValue, to = property.CurrentValue };
                    }

                    break;
            }
        }

        if (action == AuditAction.Updated && changes.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize(changes);
    }

    private sealed record PendingAuditEntry(EntityEntry Entry, string EntityName, int? EntityId, AuditAction Action, string? Changes)
    {
        // Called after the triggering entity has been saved, so Created rows can
        // read their now-resolved Id/FK values (see the comment in CaptureAuditEntries).
        public AuditLog ToAuditLog(string? changedBy, DateTime timestamp) => new()
        {
            EntityName = EntityName,
            EntityId = EntityId ?? (int)Entry.Property("Id").CurrentValue!,
            Action = Action,
            ChangedBy = changedBy,
            Timestamp = timestamp,
            Changes = Changes ?? BuildChangesJson(Entry, Action) ?? "{}",
        };
    }
}
