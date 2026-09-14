namespace ExcelManagement.Application.Employees;

public interface IEmployeeRepository
{
    Task<PagedResult<EmployeeListItemDto>> GetPagedAsync(EmployeeListQuery query, CancellationToken ct);

    /// <summary>Same filters as <see cref="GetPagedAsync"/> but returns the entire matching set, unpaged (for export).</summary>
    Task<IReadOnlyList<EmployeeListItemDto>> GetAllAsync(EmployeeListQuery query, CancellationToken ct);

    Task<EmployeeDetailDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<EmployeeDetailDto> CreateAsync(UpsertEmployeeRequest request, CancellationToken ct);

    Task<bool> UpdateAsync(int id, UpsertEmployeeRequest request, CancellationToken ct);

    Task<bool> DeleteAsync(int id, CancellationToken ct);

    /// <summary>Inserts all requests in a single SaveChangesAsync call (one implicit transaction — all-or-nothing).</summary>
    Task<int> ImportAsync(IReadOnlyList<UpsertEmployeeRequest> requests, CancellationToken ct);
}
