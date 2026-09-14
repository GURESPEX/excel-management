using System.Net;
using System.Net.Http.Json;
using ExcelManagement.Application.AuditLogs;
using ExcelManagement.Application.Departments;
using ExcelManagement.Application.Employees;
using Xunit;

namespace ExcelManagement.Api.IntegrationTests;

public class AuditLogTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => AuthTestHelper.LoginAsAdminAsync(_client);

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<int> GetAnyDepartmentIdAsync()
    {
        var departments = await _client.GetFromJsonAsync<List<DepartmentDto>>("/departments");
        return departments![0].Id;
    }

    private async Task<List<AuditLogDto>> GetLogsForAsync(string entityName, int entityId)
    {
        var logs = await _client.GetFromJsonAsync<List<AuditLogDto>>($"/audit-logs?entityName={entityName}");
        return logs!.Where(l => l.EntityId == entityId).ToList();
    }

    [Fact]
    public async Task CreateEmployee_WritesOneCreatedAuditEntry()
    {
        var departmentId = await GetAnyDepartmentIdAsync();
        var response = await _client.PostAsJsonAsync(
            "/employees",
            new UpsertEmployeeRequest("Audit Created", departmentId, 1000m, "2024-01-01", true));
        var created = await response.Content.ReadFromJsonAsync<EmployeeDetailDto>();

        var logs = await GetLogsForAsync("Employee", created!.Id);

        var entry = Assert.Single(logs);
        Assert.Equal("Created", entry.Action);
        Assert.Equal("admin", entry.ChangedBy);
        Assert.Contains("Audit Created", entry.Changes);
    }

    [Fact]
    public async Task UpdateEmployee_WritesOneUpdatedAuditEntryWithDiff()
    {
        var departmentId = await GetAnyDepartmentIdAsync();
        var createResponse = await _client.PostAsJsonAsync(
            "/employees",
            new UpsertEmployeeRequest("Before Update", departmentId, 1000m, "2024-01-01", true));
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDetailDto>();

        await _client.PutAsJsonAsync(
            $"/employees/{created!.Id}",
            new UpsertEmployeeRequest("After Update", departmentId, 2000m, "2024-01-01", true));

        var logs = await GetLogsForAsync("Employee", created.Id);

        Assert.Equal(2, logs.Count); // Created + Updated
        var updateEntry = Assert.Single(logs, l => l.Action == "Updated");
        Assert.Equal("admin", updateEntry.ChangedBy);
        Assert.Contains("Before Update", updateEntry.Changes);
        Assert.Contains("After Update", updateEntry.Changes);
    }

    [Fact]
    public async Task DeleteEmployee_WritesOneDeletedAuditEntry()
    {
        var departmentId = await GetAnyDepartmentIdAsync();
        var createResponse = await _client.PostAsJsonAsync(
            "/employees",
            new UpsertEmployeeRequest("To Be Deleted", departmentId, 1000m, "2024-01-01", true));
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDetailDto>();

        await _client.DeleteAsync($"/employees/{created!.Id}");

        var logs = await GetLogsForAsync("Employee", created.Id);

        var deleteEntry = Assert.Single(logs, l => l.Action == "Deleted");
        Assert.Equal("admin", deleteEntry.ChangedBy);
    }

    [Fact]
    public async Task CreateAndUpdateDepartment_WritesAuditEntries()
    {
        var createResponse = await _client.PostAsJsonAsync("/departments", new CreateDepartmentRequest("Audit Dept"));
        var created = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();

        await _client.PutAsJsonAsync($"/departments/{created!.Id}", new UpdateDepartmentRequest("Audit Dept Renamed", false));

        var logs = await GetLogsForAsync("Department", created.Id);

        Assert.Equal(2, logs.Count);
        Assert.Contains(logs, l => l.Action == "Created");
        Assert.Contains(logs, l => l.Action == "Updated");
    }

    [Fact]
    public async Task GetAuditLogs_FiltersByEntityNameAndDateRange()
    {
        var departmentId = await GetAnyDepartmentIdAsync();
        await _client.PostAsJsonAsync(
            "/employees",
            new UpsertEmployeeRequest("Filter Target", departmentId, 1000m, "2024-01-01", true));

        var employeeOnly = await _client.GetFromJsonAsync<List<AuditLogDto>>("/audit-logs?entityName=Employee");
        Assert.NotNull(employeeOnly);
        Assert.All(employeeOnly!, l => Assert.Equal("Employee", l.EntityName));

        var future = DateTime.UtcNow.AddDays(1).ToString("O");
        var noneYet = await _client.GetFromJsonAsync<List<AuditLogDto>>($"/audit-logs?from={future}");
        Assert.Empty(noneYet!);
    }

    [Fact]
    public async Task GetAuditLogs_AsViewer_ReturnsForbidden()
    {
        var client = factory.CreateClient();
        await AuthTestHelper.LoginAsViewerAsync(client);

        var response = await client.GetAsync("/audit-logs");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_WithoutToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/audit-logs");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
