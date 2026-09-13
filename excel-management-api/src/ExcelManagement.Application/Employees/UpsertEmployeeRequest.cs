namespace ExcelManagement.Application.Employees;

public record UpsertEmployeeRequest(
    string Name,
    int DepartmentId,
    decimal Salary,
    string JoinDate,
    bool IsActive);
