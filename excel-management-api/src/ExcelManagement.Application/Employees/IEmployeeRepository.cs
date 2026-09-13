namespace ExcelManagement.Application.Employees;

public interface IEmployeeRepository
{
    Task<PagedResult<EmployeeListItemDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct);
}
