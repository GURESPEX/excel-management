namespace ExcelManagement.Application.Departments;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<DepartmentDto>> GetAllAsync(bool includeInactive, CancellationToken ct);

    Task<bool> ExistsAsync(int id, CancellationToken ct);

    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct);

    Task<DepartmentDto> CreateAsync(string name, CancellationToken ct);

    Task<DepartmentDto?> UpdateAsync(int id, string name, bool isActive, CancellationToken ct);
}
