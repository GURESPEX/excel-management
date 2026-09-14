namespace ExcelManagement.Application.Employees;

/// <summary>Raw cell values read from one Excel row, before validation/parsing.</summary>
public record EmployeeImportRow(
    int RowNumber,
    string Name,
    string DepartmentName,
    string Salary,
    string JoinDate,
    string Status);

public record ImportRowError(int Row, string Field, string Reason);

public record EmployeeImportResult(int ImportedCount, IReadOnlyList<ImportRowError> Errors);
