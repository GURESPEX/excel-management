namespace ExcelManagement.Application.Employees;

public interface IEmployeeRepository
{
    Task<PagedResult<EmployeeListItemDto>> GetPagedAsync(EmployeeListQuery query, CancellationToken ct);

    Task<EmployeeDetailDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<EmployeeDetailDto> CreateAsync(UpsertEmployeeRequest request, CancellationToken ct);

    Task<bool> UpdateAsync(int id, UpsertEmployeeRequest request, CancellationToken ct);

    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
