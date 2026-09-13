using System.Net;
using System.Net.Http.Json;
using ExcelManagement.Application.Departments;
using ExcelManagement.Application.Employees;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ExcelManagement.Api.IntegrationTests;

public class EmployeeCrudTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<int> GetAnyDepartmentIdAsync()
    {
        var departments = await _client.GetFromJsonAsync<List<DepartmentDto>>("/departments");
        return departments![0].Id;
    }

    [Fact]
    public async Task CreateEmployee_PersistsAndReturnsCreated()
    {
        var departmentId = await GetAnyDepartmentIdAsync();
        var request = new UpsertEmployeeRequest("Test Employee", departmentId, 1000m, new DateOnly(2024, 1, 1), true);

        var response = await _client.PostAsJsonAsync("/employees", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<EmployeeDetailDto>();
        Assert.NotNull(created);
        Assert.Equal("Test Employee", created!.Name);

        var getResponse = await _client.GetAsync($"/employees/{created.Id}");
        getResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateEmployee_MissingName_ReturnsValidationProblem()
    {
        var departmentId = await GetAnyDepartmentIdAsync();
        var request = new UpsertEmployeeRequest("", departmentId, 1000m, new DateOnly(2024, 1, 1), true);

        var response = await _client.PostAsJsonAsync("/employees", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Contains("Name", problem!.Errors.Keys);
    }

    [Fact]
    public async Task CreateEmployee_UnknownDepartment_ReturnsValidationProblem()
    {
        var request = new UpsertEmployeeRequest("Someone", 999_999, 1000m, new DateOnly(2024, 1, 1), true);

        var response = await _client.PostAsJsonAsync("/employees", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Contains("DepartmentId", problem!.Errors.Keys);
    }

    [Fact]
    public async Task CreateEmployee_NegativeSalary_ReturnsValidationProblem()
    {
        var departmentId = await GetAnyDepartmentIdAsync();
        var request = new UpsertEmployeeRequest("Someone", departmentId, -1m, new DateOnly(2024, 1, 1), true);

        var response = await _client.PostAsJsonAsync("/employees", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Contains("Salary", problem!.Errors.Keys);
    }

    [Fact]
    public async Task UpdateEmployee_ChangesFieldsAndPersists()
    {
        var departmentId = await GetAnyDepartmentIdAsync();
        var createRequest = new UpsertEmployeeRequest("Original Name", departmentId, 1000m, new DateOnly(2024, 1, 1), true);
        var createResponse = await _client.PostAsJsonAsync("/employees", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDetailDto>();

        var updateRequest = new UpsertEmployeeRequest("Updated Name", departmentId, 2000m, new DateOnly(2024, 2, 2), false);
        var updateResponse = await _client.PutAsJsonAsync($"/employees/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/employees/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<EmployeeDetailDto>();
        Assert.Equal("Updated Name", updated!.Name);
        Assert.Equal(2000m, updated.Salary);
        Assert.False(updated.IsActive);
        Assert.True(updated.UpdatedAt > created.UpdatedAt);
    }

    [Fact]
    public async Task UpdateEmployee_NotFound_Returns404()
    {
        var departmentId = await GetAnyDepartmentIdAsync();
        var request = new UpsertEmployeeRequest("Nobody", departmentId, 1000m, new DateOnly(2024, 1, 1), true);

        var response = await _client.PutAsJsonAsync("/employees/999999", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_RemovesRecord()
    {
        var departmentId = await GetAnyDepartmentIdAsync();
        var createRequest = new UpsertEmployeeRequest("To Delete", departmentId, 1000m, new DateOnly(2024, 1, 1), true);
        var createResponse = await _client.PostAsJsonAsync("/employees", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDetailDto>();

        var deleteResponse = await _client.DeleteAsync($"/employees/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_NotFound_Returns404()
    {
        var response = await _client.DeleteAsync("/employees/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
