using System.Net.Http.Json;
using ExcelManagement.Application.Employees;
using Xunit;

namespace ExcelManagement.Api.IntegrationTests;

public class EmployeesEndpointTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => AuthTestHelper.LoginAsAdminAsync(_client);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetEmployees_ReturnsSeededPagedResult()
    {
        var response = await _client.GetAsync("/employees");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<EmployeeListItemDto>>();

        Assert.NotNull(result);
        Assert.Equal(5, result!.TotalCount);
        Assert.Equal(5, result.Items.Count);
        Assert.Contains(result.Items, e => e.Name == "John Doe" && e.DepartmentName == "Engineering");
    }

    [Fact]
    public async Task GetEmployees_RespectsPageSize()
    {
        var response = await _client.GetAsync("/employees?page=1&pageSize=2");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<EmployeeListItemDto>>();

        Assert.NotNull(result);
        Assert.Equal(2, result!.Items.Count);
        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public async Task GetEmployees_ClampsOutOfRangePaging()
    {
        var response = await _client.GetAsync("/employees?page=0&pageSize=-5");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<EmployeeListItemDto>>();

        Assert.NotNull(result);
        Assert.Equal(1, result!.Page);
        Assert.Equal(1, result.PageSize);
    }
}
