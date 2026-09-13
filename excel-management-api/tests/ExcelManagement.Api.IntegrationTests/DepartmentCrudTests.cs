using System.Net;
using System.Net.Http.Json;
using ExcelManagement.Application.Departments;
using ExcelManagement.Application.Employees;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ExcelManagement.Api.IntegrationTests;

public class DepartmentCrudTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => AuthTestHelper.LoginAsAdminAsync(_client);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateDepartment_PersistsAsActiveByDefault()
    {
        var response = await _client.PostAsJsonAsync("/departments", new CreateDepartmentRequest("Logistics"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<DepartmentDto>();
        Assert.NotNull(created);
        Assert.Equal("Logistics", created!.Name);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task CreateDepartment_DuplicateName_ReturnsValidationProblem()
    {
        await _client.PostAsJsonAsync("/departments", new CreateDepartmentRequest("Legal"));

        var response = await _client.PostAsJsonAsync("/departments", new CreateDepartmentRequest("Legal"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Contains("Name", problem!.Errors.Keys);
    }

    [Fact]
    public async Task UpdateDepartment_RenamesAndTogglesActive()
    {
        var createResponse = await _client.PostAsJsonAsync("/departments", new CreateDepartmentRequest("Temp Name"));
        var created = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();

        var updateResponse = await _client.PutAsJsonAsync(
            $"/departments/{created!.Id}",
            new UpdateDepartmentRequest("Renamed", false));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<DepartmentDto>();
        Assert.Equal("Renamed", updated!.Name);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task UpdateDepartment_NotFound_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/departments/999999", new UpdateDepartmentRequest("Nope", true));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDepartments_ExcludesDisabledByDefault_ButIncludesWhenRequested()
    {
        var createResponse = await _client.PostAsJsonAsync("/departments", new CreateDepartmentRequest("Disabled Dept"));
        var created = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();
        await _client.PutAsJsonAsync($"/departments/{created!.Id}", new UpdateDepartmentRequest("Disabled Dept", false));

        var activeOnly = await _client.GetFromJsonAsync<List<DepartmentDto>>("/departments");
        var withInactive = await _client.GetFromJsonAsync<List<DepartmentDto>>("/departments?includeInactive=true");

        Assert.DoesNotContain(activeOnly!, d => d.Id == created.Id);
        Assert.Contains(withInactive!, d => d.Id == created.Id && !d.IsActive);
    }

    [Fact]
    public async Task ExistingEmployee_ReferencingDisabledDepartment_StillRendersAndSaves()
    {
        var deptResponse = await _client.PostAsJsonAsync("/departments", new CreateDepartmentRequest("Soon Disabled"));
        var department = await deptResponse.Content.ReadFromJsonAsync<DepartmentDto>();

        var employeeResponse = await _client.PostAsJsonAsync(
            "/employees",
            new UpsertEmployeeRequest("Long Timer", department!.Id, 1000m, "2024-01-01", true));
        var employee = await employeeResponse.Content.ReadFromJsonAsync<EmployeeDetailDto>();

        await _client.PutAsJsonAsync($"/departments/{department.Id}", new UpdateDepartmentRequest(department.Name, false));

        var getResponse = await _client.GetAsync($"/employees/{employee!.Id}");
        getResponse.EnsureSuccessStatusCode();
        var fetched = await getResponse.Content.ReadFromJsonAsync<EmployeeDetailDto>();
        Assert.Equal(department.Id, fetched!.DepartmentId);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/employees/{employee.Id}",
            new UpsertEmployeeRequest("Long Timer", department.Id, 1200m, "2024-01-01", true));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
    }
}
