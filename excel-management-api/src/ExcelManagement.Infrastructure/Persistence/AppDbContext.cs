using ExcelManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ExcelManagement.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();

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
    }
}
