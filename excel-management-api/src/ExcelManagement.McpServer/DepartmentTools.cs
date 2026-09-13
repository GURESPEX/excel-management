using System.ComponentModel;
using ExcelManagement.Application.Departments;
using ModelContextProtocol.Server;

namespace ExcelManagement.McpServer;

[McpServerToolType]
public static class DepartmentTools
{
    [McpServerTool(Name = "list_departments"), Description(
        "Lists departments. Returns an array of DepartmentDto (id, name, isActive). " +
        "By default only active departments are returned; pass includeInactive=true to " +
        "also see inactive ones. Use the returned id values as departmentId when creating " +
        "or updating an employee.")]
    public static async Task<IReadOnlyList<DepartmentDto>> ListDepartments(
        IDepartmentRepository departments,
        [Description("Include inactive departments in the results. Defaults to false.")] bool includeInactive = false,
        CancellationToken ct = default)
    {
        return await departments.GetAllAsync(includeInactive, ct);
    }
}
