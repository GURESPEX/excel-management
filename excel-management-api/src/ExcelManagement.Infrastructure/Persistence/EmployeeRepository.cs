using ExcelManagement.Application.Employees;
using ExcelManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExcelManagement.Infrastructure.Persistence;

public class EmployeeRepository(AppDbContext db) : IEmployeeRepository
{
    public async Task<PagedResult<EmployeeListItemDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = db.Employees
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Select(e => new EmployeeListItemDto(
                e.Id,
                e.Name,
                e.Department!.Name,
                e.Salary,
                e.JoinDate,
                e.IsActive,
                e.UpdatedAt));

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<EmployeeListItemDto>(items, page, pageSize, totalCount);
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
            JoinDate = request.JoinDate,
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
        employee.JoinDate = request.JoinDate;
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

    private static EmployeeDetailDto ToDetailDto(Employee e) =>
        new(e.Id, e.Name, e.DepartmentId, e.Salary, e.JoinDate, e.IsActive, e.UpdatedAt);
}
