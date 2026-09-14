using System.Globalization;
using ExcelManagement.Application.Employees;
using ExcelManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExcelManagement.Infrastructure.Persistence;

public class EmployeeRepository(AppDbContext db) : IEmployeeRepository
{
    public async Task<PagedResult<EmployeeListItemDto>> GetPagedAsync(EmployeeListQuery filter, CancellationToken ct)
    {
        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        var projected = ProjectListItems(filter);

        var totalCount = await projected.CountAsync(ct);
        var items = await projected.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<EmployeeListItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<EmployeeListItemDto>> GetAllAsync(EmployeeListQuery filter, CancellationToken ct)
    {
        return await ProjectListItems(filter).ToListAsync(ct);
    }

    private IQueryable<EmployeeListItemDto> ProjectListItems(EmployeeListQuery filter)
    {
        return BuildFilteredQuery(filter)
            .OrderBy(e => e.Id)
            .Select(e => new EmployeeListItemDto(
                e.Id,
                e.Name,
                e.Department!.Name,
                e.Salary,
                e.JoinDate,
                e.IsActive,
                e.UpdatedAt));
    }

    private IQueryable<Employee> BuildFilteredQuery(EmployeeListQuery filter)
    {
        var query = db.Employees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            // ToLower() on both sides translates to SQL LOWER(...), keeping the
            // partial match case-insensitive on Postgres too — plain .Contains()
            // is only case-insensitive by accident on SQLite's default collation.
            var name = filter.Name.ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(name));
        }

        if (filter.DepartmentId is { } departmentId)
        {
            query = query.Where(e => e.DepartmentId == departmentId);
        }

        if (filter.IsActive is { } isActive)
        {
            query = query.Where(e => e.IsActive == isActive);
        }

        if (filter.MinSalary is { } minSalary)
        {
            query = query.Where(e => e.Salary >= minSalary);
        }

        if (filter.MaxSalary is { } maxSalary)
        {
            query = query.Where(e => e.Salary <= maxSalary);
        }

        if (TryParseJoinDate(filter.JoinDateFrom, out var joinDateFrom))
        {
            query = query.Where(e => e.JoinDate >= joinDateFrom);
        }

        if (TryParseJoinDate(filter.JoinDateTo, out var joinDateTo))
        {
            query = query.Where(e => e.JoinDate <= joinDateTo);
        }

        return query;
    }

    private static bool TryParseJoinDate(string? value, out DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            date = default;
            return false;
        }

        return DateOnly.TryParseExact(value, EmployeeValidator.JoinDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    public async Task<EmployeeDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Employees
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EmployeeDetailDto(e.Id, e.Name, e.DepartmentId, e.Salary, e.JoinDate, e.IsActive, e.UpdatedAt))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<EmployeeDetailDto> CreateAsync(UpsertEmployeeRequest request, CancellationToken ct)
    {
        var employee = new Employee
        {
            Name = request.Name,
            DepartmentId = request.DepartmentId,
            Salary = request.Salary,
            JoinDate = DateOnly.ParseExact(request.JoinDate, EmployeeValidator.JoinDateFormat, CultureInfo.InvariantCulture),
            IsActive = request.IsActive,
            UpdatedAt = DateTime.UtcNow,
        };

        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);

        return ToDetailDto(employee);
    }

    public async Task<bool> UpdateAsync(int id, UpsertEmployeeRequest request, CancellationToken ct)
    {
        var employee = await db.Employees.SingleOrDefaultAsync(e => e.Id == id, ct);
        if (employee is null)
        {
            return false;
        }

        employee.Name = request.Name;
        employee.DepartmentId = request.DepartmentId;
        employee.Salary = request.Salary;
        employee.JoinDate = DateOnly.ParseExact(request.JoinDate, EmployeeValidator.JoinDateFormat, CultureInfo.InvariantCulture);
        employee.IsActive = request.IsActive;
        employee.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var employee = await db.Employees.SingleOrDefaultAsync(e => e.Id == id, ct);
        if (employee is null)
        {
            return false;
        }

        db.Employees.Remove(employee);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<int> ImportAsync(IReadOnlyList<UpsertEmployeeRequest> requests, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var entities = requests.Select(r => new Employee
        {
            Name = r.Name,
            DepartmentId = r.DepartmentId,
            Salary = r.Salary,
            JoinDate = DateOnly.ParseExact(r.JoinDate, EmployeeValidator.JoinDateFormat, CultureInfo.InvariantCulture),
            IsActive = r.IsActive,
            UpdatedAt = now,
        }).ToList();

        db.Employees.AddRange(entities);
        await db.SaveChangesAsync(ct);

        return entities.Count;
    }

    private static EmployeeDetailDto ToDetailDto(Employee e) =>
        new(e.Id, e.Name, e.DepartmentId, e.Salary, e.JoinDate, e.IsActive, e.UpdatedAt);
}
