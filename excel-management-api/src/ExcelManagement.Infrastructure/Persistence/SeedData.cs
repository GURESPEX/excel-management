using ExcelManagement.Domain;

namespace ExcelManagement.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await Task.FromResult(db.Departments.Any()))
        {
            return;
        }

        var engineering = new Department { Name = "Engineering", IsActive = true };
        var marketing = new Department { Name = "Marketing", IsActive = true };
        var sales = new Department { Name = "Sales", IsActive = true };
        var hr = new Department { Name = "HR", IsActive = true };

        db.Departments.AddRange(engineering, marketing, sales, hr);

        db.Employees.AddRange(
            new Employee { Id = 101, Name = "John Doe", Department = engineering, Salary = 65000m, JoinDate = new DateOnly(2023, 1, 15), IsActive = true, UpdatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Employee { Id = 102, Name = "Jane Smith", Department = marketing, Salary = 58000m, JoinDate = new DateOnly(2023, 3, 22), IsActive = true, UpdatedAt = new DateTime(2025, 12, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Employee { Id = 103, Name = "Alice Wong", Department = sales, Salary = 45000m, JoinDate = new DateOnly(2024, 6, 1), IsActive = true, UpdatedAt = new DateTime(2026, 2, 14, 0, 0, 0, DateTimeKind.Utc) },
            new Employee { Id = 104, Name = "Bob Brown", Department = engineering, Salary = 72000m, JoinDate = new DateOnly(2022, 11, 10), IsActive = false, UpdatedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Employee { Id = 105, Name = "Charlie Day", Department = hr, Salary = 50000m, JoinDate = new DateOnly(2024, 2, 19), IsActive = true, UpdatedAt = new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc) });

        await db.SaveChangesAsync();
    }
}
