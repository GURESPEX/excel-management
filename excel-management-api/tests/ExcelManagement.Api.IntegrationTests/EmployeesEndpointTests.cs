using System.Net.Http.Json;
using ExcelManagement.Application.Employees;
using Xunit;

namespace ExcelManagement.Api.IntegrationTests;

public class EmployeesEndpointTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    [Fact]
    public async Task GetEmployees_ReturnsSeededPagedResult()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/employees");
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
        var client = factory.CreateClient();

        var response = await client.GetAsync("/employees?page=1&pageSize=2");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<EmployeeListItemDto>>();

        Assert.NotNull(result);
        Assert.Equal(2, result!.Items.Count);
        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public async Task GetEmployees_ClampsOutOfRangePaging()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/employees?page=0&pageSize=-5");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<EmployeeListItemDto>>();

        Assert.NotNull(result);
        Assert.Equal(1, result!.Page);
        Assert.Equal(1, result.PageSize);
    }
}
