namespace ExcelManagement.Application.Employees;

public record EmployeeListQuery(
    int Page = 1,
    int PageSize = 20,
    string? Name = null,
    int? DepartmentId = null,
    bool? IsActive = null,
    decimal? MinSalary = null,
    decimal? MaxSalary = null,
    string? JoinDateFrom = null,
    string? JoinDateTo = null);
