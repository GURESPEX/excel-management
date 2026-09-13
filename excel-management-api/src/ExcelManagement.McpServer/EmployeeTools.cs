using System.ComponentModel;
using ExcelManagement.Application.Departments;
using ExcelManagement.Application.Employees;
using ModelContextProtocol.Server;

namespace ExcelManagement.McpServer;

[McpServerToolType]
public static class EmployeeTools
{
    [McpServerTool(Name = "list_employees"), Description(
        "Lists employees page by page. Returns a PagedResult with fields: " +
        "items (array of employee list rows: id, name, departmentName, salary, joinDate " +
        "as yyyy-MM-dd, isActive, updatedAt), page, pageSize, totalCount. " +
        "There is no free-text search; use page/pageSize to page through all rows.")]
    public static async Task<PagedResult<EmployeeListItemDto>> ListEmployees(
        IEmployeeRepository employees,
        [Description("1-based page number. Defaults to 1.")] int page = 1,
        [Description("Rows per page. Server clamps to the 1-200 range. Defaults to 20.")] int pageSize = 20,
        CancellationToken ct = default)
    {
        return await employees.GetPagedAsync(new EmployeeListQuery(page, pageSize), ct);
    }

    [McpServerTool(Name = "get_employee"), Description(
        "Gets a single employee by id. Returns an EmployeeDetailDto (id, name, departmentId, " +
        "salary, joinDate as yyyy-MM-dd, isActive, updatedAt) or null if no employee with that id exists.")]
    public static async Task<EmployeeDetailDto?> GetEmployee(
        IEmployeeRepository employees,
        [Description("The employee's id.")] int id,
        CancellationToken ct = default)
    {
        return await employees.GetByIdAsync(id, ct);
    }

    [McpServerTool(Name = "create_employee"), Description(
        "Creates a new employee. Fields: name (string, required, non-empty), " +
        "departmentId (int, must reference an existing department returned by list_departments — " +
        "note it does not need to be active), salary (decimal, must be >= 0), " +
        "joinDate (string, must be an exact yyyy-MM-dd date, e.g. \"2024-01-31\"), " +
        "isActive (bool). On validation failure returns the field-level error messages instead " +
        "of throwing; on success returns the created EmployeeDetailDto.")]
    public static async Task<CreateEmployeeResult> CreateEmployee(
        IEmployeeRepository employees,
        IDepartmentRepository departments,
        [Description("Employee's full name. Required, non-empty.")] string name,
        [Description("Id of an existing department (see list_departments).")] int departmentId,
        [Description("Salary. Must be zero or greater.")] decimal salary,
        [Description("Join date formatted exactly yyyy-MM-dd, e.g. 2024-01-31.")] string joinDate,
        [Description("Whether the employee is currently active.")] bool isActive,
        CancellationToken ct = default)
    {
        var request = new UpsertEmployeeRequest(name, departmentId, salary, joinDate, isActive);
        var errors = await ValidateAsync(request, departments, ct);
        if (errors.Count > 0)
        {
            return new CreateEmployeeResult(null, errors);
        }

        var created = await employees.CreateAsync(request, ct);
        return new CreateEmployeeResult(created, null);
    }

    [McpServerTool(Name = "update_employee"), Description(
        "Updates an existing employee identified by id. Same field rules as create_employee: " +
        "name (required, non-empty), departmentId (must reference an existing department), " +
        "salary (>= 0), joinDate (exact yyyy-MM-dd), isActive (bool). All fields are replaced " +
        "(not a partial patch) — pass the employee's current values for any field you don't want " +
        "to change. Returns updated=true on success, updated=false if no employee has that id, " +
        "or field-level validation errors.")]
    public static async Task<UpdateEmployeeResult> UpdateEmployee(
        IEmployeeRepository employees,
        IDepartmentRepository departments,
        [Description("Id of the employee to update.")] int id,
        [Description("Employee's full name. Required, non-empty.")] string name,
        [Description("Id of an existing department (see list_departments).")] int departmentId,
        [Description("Salary. Must be zero or greater.")] decimal salary,
        [Description("Join date formatted exactly yyyy-MM-dd, e.g. 2024-01-31.")] string joinDate,
        [Description("Whether the employee is currently active.")] bool isActive,
        CancellationToken ct = default)
    {
        var request = new UpsertEmployeeRequest(name, departmentId, salary, joinDate, isActive);
        var errors = await ValidateAsync(request, departments, ct);
        if (errors.Count > 0)
        {
            return new UpdateEmployeeResult(false, errors);
        }

        var updated = await employees.UpdateAsync(id, request, ct);
        return new UpdateEmployeeResult(updated, null);
    }

    [McpServerTool(Name = "delete_employee"), Description(
        "Deletes an employee by id. Returns deleted=true on success, deleted=false if no " +
        "employee with that id exists.")]
    public static async Task<bool> DeleteEmployee(
        IEmployeeRepository employees,
        [Description("Id of the employee to delete.")] int id,
        CancellationToken ct = default)
    {
        return await employees.DeleteAsync(id, ct);
    }

    [McpServerTool(Name = "export_employees"), Description(
        "Returns the full employee dataset (every row, not just one page) as structured JSON — " +
        "an array of employee list rows (id, name, departmentName, salary, joinDate as yyyy-MM-dd, " +
        "isActive, updatedAt). This is a data export for use by an AI client or downstream tool, " +
        "not a generated .xlsx file.")]
    public static async Task<IReadOnlyList<EmployeeListItemDto>> ExportEmployees(
        IEmployeeRepository employees,
        CancellationToken ct = default)
    {
        const int pageSize = 200;
        var all = new List<EmployeeListItemDto>();
        var page = 1;
        while (true)
        {
            var result = await employees.GetPagedAsync(new EmployeeListQuery(page, pageSize), ct);
            all.AddRange(result.Items);
            if (all.Count >= result.TotalCount || result.Items.Count == 0)
            {
                break;
            }

            page++;
        }

        return all;
    }

    // Reuses the exact same validation path the REST API's ValidateEmployeeRequestAsync
    // helper uses (see ExcelManagement.Api/Program.cs) — department-existence check via
    // IDepartmentRepository plus EmployeeValidator — so business rules aren't reimplemented.
    private static async Task<Dictionary<string, string[]>> ValidateAsync(
        UpsertEmployeeRequest request, IDepartmentRepository departments, CancellationToken ct)
    {
        var departmentExists = await departments.ExistsAsync(request.DepartmentId, ct);
        return EmployeeValidator.Validate(request, departmentExists);
    }
}

public record CreateEmployeeResult(EmployeeDetailDto? Employee, Dictionary<string, string[]>? Errors);

public record UpdateEmployeeResult(bool Updated, Dictionary<string, string[]>? Errors);
