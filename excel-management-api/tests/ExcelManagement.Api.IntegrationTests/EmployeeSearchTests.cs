using System.Net.Http.Json;
using ExcelManagement.Application.Departments;
using ExcelManagement.Application.Employees;
using Xunit;

namespace ExcelManagement.Api.IntegrationTests;

public class EmployeeSearchTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => AuthTestHelper.LoginAsAdminAsync(_client);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Name_PartialMatch_ReturnsOnlyMatchingEmployees()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<EmployeeListItemDto>>("/employees?name=oh");

        Assert.NotNull(result);
        Assert.All(result!.Items, e => Assert.Contains("oh", e.Name, StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Items, e => e.Name == "John Doe");
    }

    [Fact]
    public async Task Department_AndStatus_Combine_ReturnsOnlyMatchingEmployees()
    {
        var departments = await _client.GetFromJsonAsync<List<DepartmentDto>>("/departments");
        var engineeringId = departments!.Single(d => d.Name == "Engineering").Id;

        // Engineering has both an active (John Doe) and inactive (Bob Brown) employee seeded;
        // combining departmentId + isActive=true must exclude the inactive one (AND, not OR).
        var result = await _client.GetFromJsonAsync<PagedResult<EmployeeListItemDto>>(
            $"/employees?departmentId={engineeringId}&isActive=true");

        Assert.NotNull(result);
        Assert.All(result!.Items, e => Assert.Equal("Engineering", e.DepartmentName));
        Assert.All(result.Items, e => Assert.True(e.IsActive));
        Assert.Contains(result.Items, e => e.Name == "John Doe");
        Assert.DoesNotContain(result.Items, e => e.Name == "Bob Brown");
    }

    [Fact]
    public async Task SalaryRange_ReturnsOnlyEmployeesWithinRange()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<EmployeeListItemDto>>(
            "/employees?minSalary=50000&maxSalary=60000");

        Assert.NotNull(result);
        Assert.All(result!.Items, e => Assert.InRange(e.Salary, 50000m, 60000m));
        Assert.Contains(result.Items, e => e.Name == "Jane Smith");
        Assert.DoesNotContain(result.Items, e => e.Name == "John Doe");
    }

    [Fact]
    public async Task JoinDateRange_ReturnsOnlyEmployeesWithinRange()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<EmployeeListItemDto>>(
            "/employees?joinDateFrom=2024-01-01&joinDateTo=2024-12-31");

        Assert.NotNull(result);
        Assert.All(result!.Items, e => Assert.InRange(e.JoinDate, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31)));
        Assert.Contains(result.Items, e => e.Name == "Alice Wong");
        Assert.DoesNotContain(result.Items, e => e.Name == "John Doe");
    }
}
