namespace ExcelManagement.Application.Employees;

public record UpsertEmployeeRequest(
    string Name,
    int DepartmentId,
    decimal Salary,
    DateOnly JoinDate,
    bool IsActive);
