using System.Globalization;

namespace ExcelManagement.Application.Employees;

public static class EmployeeValidator
{
    public const string JoinDateFormat = "yyyy-MM-dd";

    public static Dictionary<string, string[]> Validate(UpsertEmployeeRequest request, bool departmentExists)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(request.Name)] = ["Name is required."];
        }

        if (!departmentExists)
        {
            errors[nameof(request.DepartmentId)] = ["Department does not exist."];
        }

        if (request.Salary < 0)
        {
            errors[nameof(request.Salary)] = ["Salary must be non-negative."];
        }

        if (!DateOnly.TryParseExact(request.JoinDate, JoinDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            errors[nameof(request.JoinDate)] = ["Join Date is not a valid date."];
        }

        return errors;
    }
}
