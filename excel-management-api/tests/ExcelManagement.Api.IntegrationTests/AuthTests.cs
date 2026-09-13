using System.Net;
using System.Net.Http.Json;
using ExcelManagement.Application.Auth;
using ExcelManagement.Application.Departments;
using ExcelManagement.Application.Employees;
using Xunit;

namespace ExcelManagement.Api.IntegrationTests;

public class AuthTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    [Fact]
    public async Task Login_ValidAdminCredentials_ReturnsOkWithUserInfo()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest("admin", "admin123"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<AuthenticatedUserDto>();
        Assert.Equal("admin", user!.Username);
        Assert.Equal("Admin", user.Role);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest("admin", "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownUsername_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest("nobody", "whatever"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshCookie_RotatesAccessToken()
    {
        var client = factory.CreateClient();
        await AuthTestHelper.LoginAsAdminAsync(client);

        var response = await client.PostAsync("/auth/refresh", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<AuthenticatedUserDto>();
        Assert.Equal("admin", user!.Username);

        // The rotated access token must actually work for a subsequent authenticated call.
        var employeesResponse = await client.GetAsync("/employees");
        employeesResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Refresh_WithoutRefreshCookie_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/auth/refresh", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_WithoutToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/employees");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_AsViewer_Succeeds()
    {
        var client = factory.CreateClient();
        await AuthTestHelper.LoginAsViewerAsync(client);

        var response = await client.GetAsync("/employees");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateEmployee_AsViewer_ReturnsForbidden()
    {
        var client = factory.CreateClient();
        await AuthTestHelper.LoginAsViewerAsync(client);
        var departmentId = (await client.GetFromJsonAsync<List<DepartmentDto>>("/departments"))![0].Id;

        var response = await client.PostAsJsonAsync(
            "/employees",
            new { Name = "Should Fail", DepartmentId = departmentId, Salary = 1000m, JoinDate = "2024-01-01", IsActive = true });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_AsViewer_ReturnsForbidden()
    {
        var client = factory.CreateClient();
        await AuthTestHelper.LoginAsViewerAsync(client);

        var response = await client.PostAsJsonAsync("/departments", new CreateDepartmentRequest("Viewer Attempt"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_AsAdmin_Succeeds()
    {
        var client = factory.CreateClient();
        await AuthTestHelper.LoginAsAdminAsync(client);
        var departmentId = (await client.GetFromJsonAsync<List<DepartmentDto>>("/departments"))![0].Id;

        var response = await client.PostAsJsonAsync(
            "/employees",
            new { Name = "Admin Created", DepartmentId = departmentId, Salary = 1000m, JoinDate = "2024-01-01", IsActive = true });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEmployee_AsViewer_ReturnsForbidden()
    {
        var adminClient = factory.CreateClient();
        await AuthTestHelper.LoginAsAdminAsync(adminClient);
        var departmentId = (await adminClient.GetFromJsonAsync<List<DepartmentDto>>("/departments"))![0].Id;
        var created = await adminClient.PostAsJsonAsync(
            "/employees",
            new { Name = "Update Target", DepartmentId = departmentId, Salary = 1000m, JoinDate = "2024-01-01", IsActive = true });
        var employeeId = (await created.Content.ReadFromJsonAsync<EmployeeDetailDto>())!.Id;

        var viewerClient = factory.CreateClient();
        await AuthTestHelper.LoginAsViewerAsync(viewerClient);

        var response = await viewerClient.PutAsJsonAsync(
            $"/employees/{employeeId}",
            new { Name = "Should Not Update", DepartmentId = departmentId, Salary = 2000m, JoinDate = "2024-01-01", IsActive = true });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_AsViewer_ReturnsForbidden()
    {
        var adminClient = factory.CreateClient();
        await AuthTestHelper.LoginAsAdminAsync(adminClient);
        var departmentId = (await adminClient.GetFromJsonAsync<List<DepartmentDto>>("/departments"))![0].Id;
        var created = await adminClient.PostAsJsonAsync(
            "/employees",
            new { Name = "Delete Target", DepartmentId = departmentId, Salary = 1000m, JoinDate = "2024-01-01", IsActive = true });
        var employeeId = (await created.Content.ReadFromJsonAsync<EmployeeDetailDto>())!.Id;

        var viewerClient = factory.CreateClient();
        await AuthTestHelper.LoginAsViewerAsync(viewerClient);

        var response = await viewerClient.DeleteAsync($"/employees/{employeeId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDepartment_AsViewer_ReturnsForbidden()
    {
        var adminClient = factory.CreateClient();
        await AuthTestHelper.LoginAsAdminAsync(adminClient);
        var departmentId = (await adminClient.GetFromJsonAsync<List<DepartmentDto>>("/departments"))![0].Id;

        var viewerClient = factory.CreateClient();
        await AuthTestHelper.LoginAsViewerAsync(viewerClient);

        var response = await viewerClient.PutAsJsonAsync(
            $"/departments/{departmentId}",
            new UpdateDepartmentRequest("Should Not Rename", true));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_WithMalformedToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "access_token=this-is-not-a-valid-jwt");

        var response = await client.GetAsync("/employees");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
