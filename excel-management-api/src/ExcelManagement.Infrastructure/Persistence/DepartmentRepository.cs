using ExcelManagement.Application.Departments;
using ExcelManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExcelManagement.Infrastructure.Persistence;

public class DepartmentRepository(AppDbContext db) : IDepartmentRepository
{
    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync(bool includeInactive, CancellationToken ct)
    {
        var query = db.Departments.AsNoTracking().AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(d => d.IsActive);
        }

        return await query
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(d.Id, d.Name, d.IsActive))
            .ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct)
    {
        return db.Departments.AnyAsync(d => d.Id == id, ct);
    }

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct)
    {
        return db.Departments.AnyAsync(d => d.Name == name && d.Id != (excludeId ?? -1), ct);
    }

    public async Task<DepartmentDto> CreateAsync(string name, CancellationToken ct)
    {
        var department = new Department { Name = name, IsActive = true };
        db.Departments.Add(department);
        await db.SaveChangesAsync(ct);

        return new DepartmentDto(department.Id, department.Name, department.IsActive);
    }

    public async Task<DepartmentDto?> UpdateAsync(int id, string name, bool isActive, CancellationToken ct)
    {
        var department = await db.Departments.SingleOrDefaultAsync(d => d.Id == id, ct);
        if (department is null)
        {
            return null;
        }

        department.Name = name;
        department.IsActive = isActive;
        await db.SaveChangesAsync(ct);

        return new DepartmentDto(department.Id, department.Name, department.IsActive);
    }
}
