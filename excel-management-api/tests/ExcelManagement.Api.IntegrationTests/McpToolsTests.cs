using ExcelManagement.Application.Departments;
using ExcelManagement.Application.Employees;
using ExcelManagement.McpServer;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ExcelManagement.Api.IntegrationTests;

/// <summary>
/// Smoke tests: each MCP tool method, called directly (no stdio/JSON-RPC involved), reaches the
/// same Application-layer repositories the REST API uses and returns the expected shape. Business
/// rules themselves (validation edge cases, etc.) are already covered by the REST API's tests.
/// </summary>
public class McpToolsTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private (IEmployeeRepository Employees, IDepartmentRepository Departments) CreateRepositories()
    {
        var scope = factory.Services.CreateScope();
        return (
            scope.ServiceProvider.GetRequiredService<IEmployeeRepository>(),
            scope.ServiceProvider.GetRequiredService<IDepartmentRepository>());
    }

    [Fact]
    public async Task ListDepartments_DispatchesToRepository_ReturnsSeededActiveDepartments()
    {
        var (_, departments) = CreateRepositories();

        var result = await DepartmentTools.ListDepartments(departments, includeInactive: false, CancellationToken.None);

        Assert.NotEmpty(result);
        Assert.All(result, d => Assert.True(d.IsActive));
    }

    [Fact]
    public async Task ListEmployees_DispatchesToRepository_ReturnsPagedShape()
    {
        var (employees, _) = CreateRepositories();

        var result = await EmployeeTools.ListEmployees(employees, page: 1, pageSize: 2, CancellationToken.None);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.Items.Count);
        Assert.True(result.TotalCount >= result.Items.Count);
    }

    [Fact]
    public async Task GetEmployee_DispatchesToRepository_ReturnsDetailForSeededRow()
    {
        var (employees, _) = CreateRepositories();
        var page = await EmployeeTools.ListEmployees(employees, 1, 1, CancellationToken.None);
        var seededId = page.Items[0].Id;

        var detail = await EmployeeTools.GetEmployee(employees, seededId, CancellationToken.None);

        Assert.NotNull(detail);
        Assert.Equal(seededId, detail!.Id);
    }

    [Fact]
    public async Task GetEmployee_UnknownId_ReturnsNull()
    {
        var (employees, _) = CreateRepositories();

        var detail = await EmployeeTools.GetEmployee(employees, 999_999, CancellationToken.None);

        Assert.Null(detail);
    }

    [Fact]
    public async Task CreateEmployee_ValidRequest_PersistsThroughRepository()
    {
        var (employees, departments) = CreateRepositories();
        var departmentId = (await DepartmentTools.ListDepartments(departments, false, CancellationToken.None))[0].Id;

        var result = await EmployeeTools.CreateEmployee(
            employees, departments, "MCP Smoke Test", departmentId, 1234m, "2024-05-06", true, CancellationToken.None);

        Assert.Null(result.Errors);
        Assert.NotNull(result.Employee);
        Assert.Equal("MCP Smoke Test", result.Employee!.Name);
        Assert.Equal(new DateOnly(2024, 5, 6), result.Employee.JoinDate);

        // Confirm the tool actually reached the repository layer (not an in-memory echo).
        var persisted = await employees.GetByIdAsync(result.Employee.Id, CancellationToken.None);
        Assert.NotNull(persisted);
        Assert.Equal("MCP Smoke Test", persisted!.Name);
    }

    [Fact]
    public async Task CreateEmployee_UnknownDepartment_ReturnsValidationErrorsAndDoesNotCreate()
    {
        var (employees, departments) = CreateRepositories();

        var result = await EmployeeTools.CreateEmployee(
            employees, departments, "Nobody", 999_999, 100m, "2024-01-01", true, CancellationToken.None);

        Assert.Null(result.Employee);
        Assert.NotNull(result.Errors);
        Assert.Contains("DepartmentId", result.Errors!.Keys);
    }

    [Fact]
    public async Task UpdateEmployee_ValidRequest_ChangesPersistedRow()
    {
        var (employees, departments) = CreateRepositories();
        var departmentId = (await DepartmentTools.ListDepartments(departments, false, CancellationToken.None))[0].Id;
        var created = await EmployeeTools.CreateEmployee(
            employees, departments, "Before Update", departmentId, 100m, "2024-01-01", true, CancellationToken.None);

        var result = await EmployeeTools.UpdateEmployee(
            employees, departments, created.Employee!.Id, "After Update", departmentId, 200m, "2024-02-02", false, CancellationToken.None);

        Assert.True(result.Updated);
        Assert.Null(result.Errors);
        var persisted = await employees.GetByIdAsync(created.Employee.Id, CancellationToken.None);
        Assert.Equal("After Update", persisted!.Name);
        Assert.Equal(200m, persisted.Salary);
        Assert.False(persisted.IsActive);
    }

    [Fact]
    public async Task UpdateEmployee_UnknownId_ReturnsUpdatedFalse()
    {
        var (employees, departments) = CreateRepositories();
        var departmentId = (await DepartmentTools.ListDepartments(departments, false, CancellationToken.None))[0].Id;

        var result = await EmployeeTools.UpdateEmployee(
            employees, departments, 999_999, "Nobody", departmentId, 100m, "2024-01-01", true, CancellationToken.None);

        Assert.False(result.Updated);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task DeleteEmployee_ExistingRow_RemovesItAndReportsTrue()
    {
        var (employees, departments) = CreateRepositories();
        var departmentId = (await DepartmentTools.ListDepartments(departments, false, CancellationToken.None))[0].Id;
        var created = await EmployeeTools.CreateEmployee(
            employees, departments, "To Delete", departmentId, 100m, "2024-01-01", true, CancellationToken.None);

        var deleted = await EmployeeTools.DeleteEmployee(employees, created.Employee!.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Null(await employees.GetByIdAsync(created.Employee.Id, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteEmployee_UnknownId_ReturnsFalse()
    {
        var (employees, _) = CreateRepositories();

        var deleted = await EmployeeTools.DeleteEmployee(employees, 999_999, CancellationToken.None);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ExportEmployees_ReturnsEveryRowAcrossPages()
    {
        var (employees, departments) = CreateRepositories();
        var departmentId = (await DepartmentTools.ListDepartments(departments, false, CancellationToken.None))[0].Id;
        var totalBefore = (await EmployeeTools.ListEmployees(employees, 1, 1, CancellationToken.None)).TotalCount;
        await EmployeeTools.CreateEmployee(
            employees, departments, "Export Check", departmentId, 100m, "2024-01-01", true, CancellationToken.None);

        var exported = await EmployeeTools.ExportEmployees(employees, CancellationToken.None);

        Assert.Equal(totalBefore + 1, exported.Count);
        Assert.Contains(exported, e => e.Name == "Export Check");
    }
}
