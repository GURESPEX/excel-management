namespace ExcelManagement.Application.Departments;

public static class DepartmentValidator
{
    public static Dictionary<string, string[]> Validate(string name, bool nameConflict)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors["Name"] = ["Name is required."];
        }
        else if (nameConflict)
        {
            errors["Name"] = ["A department with this name already exists."];
        }

        return errors;
    }
}
