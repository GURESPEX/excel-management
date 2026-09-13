using ExcelManagement.Application.Departments;
using Microsoft.EntityFrameworkCore;

namespace ExcelManagement.Infrastructure.Persistence;

public class DepartmentRepository(AppDbContext db) : IDepartmentRepository
{
    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync(CancellationToken ct)
    {
        return await db.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(d.Id, d.Name))
            .ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct)
    {
        return db.Departments.AnyAsync(d => d.Id == id, ct);
    }
}
