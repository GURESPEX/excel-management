namespace ExcelManagement.Application.Departments;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<DepartmentDto>> GetAllAsync(CancellationToken ct);

    Task<bool> ExistsAsync(int id, CancellationToken ct);
}
