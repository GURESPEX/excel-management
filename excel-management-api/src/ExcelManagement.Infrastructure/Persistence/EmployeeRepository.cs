using ExcelManagement.Application.Employees;
using Microsoft.EntityFrameworkCore;

namespace ExcelManagement.Infrastructure.Persistence;

public class EmployeeRepository(AppDbContext db) : IEmployeeRepository
{
    public async Task<PagedResult<EmployeeListItemDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
    {
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
}
