using Microsoft.EntityFrameworkCore;
using ExcelManagement.Api.Data;
using ExcelManagement.Api.Models;

namespace ExcelManagement.Api.Services;

public class EmployeeService(AppDbContext db)
{
    public Task<List<Employee>> ListAsync() =>
        db.Employees.AsNoTracking().ToListAsync();

    public Task<Employee?> GetAsync(int id) =>
        db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Employee> CreateAsync(Employee employee)
    {
        employee.Id = 0;
        employee.LastUpdatedDate = DateTime.UtcNow;
        db.Employees.Add(employee);
        await db.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee?> UpdateAsync(int id, Employee updated)
    {
        var existing = await db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (existing is null) return null;

        existing.Name = updated.Name;
        existing.Department = updated.Department;
        existing.Salary = updated.Salary;
        existing.JoinDate = updated.JoinDate;
        existing.Status = updated.Status;
        existing.LastUpdatedDate = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (existing is null) return false;

        db.Employees.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
