namespace ExcelManagement.Application.Employees;

public record EmployeeDetailDto(
    int Id,
    string Name,
    int DepartmentId,
    decimal Salary,
    DateOnly JoinDate,
    bool IsActive,
    DateTime UpdatedAt);
