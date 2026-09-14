using System.Globalization;

namespace ExcelManagement.Application.Employees;

public static class EmployeeImportRowValidator
{
    /// <summary>
    /// Validates one raw import row: resolves the department by name, parses Salary/Status,
    /// then reuses <see cref="EmployeeValidator"/> for the shared Name/Department/Salary/JoinDate rules
    /// so import enforces exactly the same rules as Employee CRUD.
    /// </summary>
    public static (IReadOnlyList<ImportRowError> Errors, UpsertEmployeeRequest? Request) Validate(
        EmployeeImportRow row, IReadOnlyDictionary<string, int> activeDepartmentIdsByName)
    {
        var errors = new List<ImportRowError>();

        var departmentExists = activeDepartmentIdsByName.TryGetValue(row.DepartmentName.Trim(), out var departmentId);

        // Default invalid Salary to 0 (non-negative) rather than a negative sentinel, so
        // EmployeeValidator's "Salary must be non-negative" rule doesn't fire a duplicate error.
        var salaryValid = decimal.TryParse(row.Salary, NumberStyles.Number, CultureInfo.InvariantCulture, out var salary);
        if (!salaryValid)
        {
            errors.Add(new ImportRowError(row.RowNumber, nameof(UpsertEmployeeRequest.Salary), "Salary must be a valid number."));
        }

        var status = ParseStatus(row.Status);
        if (status is null)
        {
            errors.Add(new ImportRowError(row.RowNumber, "Status", "Status must be 'Active' or 'In Active'."));
        }

        var request = new UpsertEmployeeRequest(
            row.Name,
            departmentExists ? departmentId : -1,
            salaryValid ? salary : 0,
            row.JoinDate,
            status ?? false);

        foreach (var (field, messages) in EmployeeValidator.Validate(request, departmentExists))
        {
            errors.AddRange(messages.Select(message => new ImportRowError(row.RowNumber, field, message)));
        }

        return errors.Count == 0 ? (errors, request) : (errors, null);
    }

    private static bool? ParseStatus(string status)
    {
        var value = status.Trim();
        if (string.Equals(value, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(value, "In Active", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "Inactive", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return null;
    }
}
