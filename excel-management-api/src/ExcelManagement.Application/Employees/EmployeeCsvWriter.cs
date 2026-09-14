using System.Globalization;
using System.Text;

namespace ExcelManagement.Application.Employees;

/// <summary>Plain CSV writer for the Employee export — no library needed for a few quoted columns.</summary>
public static class EmployeeCsvWriter
{
    private static readonly string[] Headers = ["Id", "Name", "Department", "Salary", "Join Date", "Status"];

    public static string Write(IReadOnlyList<EmployeeListItemDto> employees)
    {
        var sb = new StringBuilder();
        sb.Append(string.Join(',', Headers)).Append("\r\n");

        foreach (var e in employees)
        {
            sb.Append(string.Join(
                ',',
                Quote(e.Id.ToString(CultureInfo.InvariantCulture)),
                Quote(e.Name),
                Quote(e.DepartmentName),
                Quote(e.Salary.ToString(CultureInfo.InvariantCulture)),
                Quote(e.JoinDate.ToString(EmployeeValidator.JoinDateFormat)),
                Quote(e.IsActive ? "Active" : "In Active")));
            sb.Append("\r\n");
        }

        return sb.ToString();
    }

    private static string Quote(string value)
    {
        if (value.IndexOfAny([',', '"', '\n', '\r']) < 0)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
