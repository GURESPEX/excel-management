namespace ExcelManagement.Application.Employees;

public record EmployeeListItemDto(
    int Id,
    string Name,
    string DepartmentName,
    decimal Salary,
    DateOnly JoinDate,
    bool IsActive,
    DateTime UpdatedAt);
