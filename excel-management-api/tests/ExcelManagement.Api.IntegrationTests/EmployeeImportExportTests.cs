using System.Net;
using System.Net.Http.Json;
using ClosedXML.Excel;
using ExcelManagement.Application.Departments;
using ExcelManagement.Application.Employees;
using Xunit;

namespace ExcelManagement.Api.IntegrationTests;

public class EmployeeImportExportTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => AuthTestHelper.LoginAsAdminAsync(_client);

    public Task DisposeAsync() => Task.CompletedTask;

    private static byte[] BuildWorkbook(IEnumerable<(string Name, string Department, string Salary, string JoinDate, string Status)> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Employees");
        string[] headers = ["Name", "Department", "Salary", "Join Date", "Status"];
        for (var c = 0; c < headers.Length; c++)
        {
            sheet.Cell(1, c + 1).Value = headers[c];
        }

        var r = 2;
        foreach (var row in rows)
        {
            sheet.Cell(r, 1).Value = row.Name;
            sheet.Cell(r, 2).Value = row.Department;
            sheet.Cell(r, 3).Value = row.Salary;
            sheet.Cell(r, 4).Value = row.JoinDate;
            sheet.Cell(r, 5).Value = row.Status;
            r++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static HttpContent ToFileContent(byte[] bytes)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", "import.xlsx");
        return content;
    }

    [Fact]
    public async Task Import_AllRowsValid_CommitsAllRows()
    {
        var bytes = BuildWorkbook([
            ("New Hire One", "Engineering", "55000", "2024-05-01", "Active"),
            ("New Hire Two", "Marketing", "48000", "2024-06-15", "In Active"),
        ]);

        var response = await _client.PostAsync("/employees/import", ToFileContent(bytes));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<EmployeeImportResult>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.ImportedCount);
        Assert.Empty(result.Errors);

        var list = await _client.GetFromJsonAsync<PagedResult<EmployeeListItemDto>>($"/employees?name={Uri.EscapeDataString("New Hire")}&pageSize=50");
        Assert.NotNull(list);
        Assert.Contains(list!.Items, e => e.Name == "New Hire One" && e.IsActive);
        Assert.Contains(list.Items, e => e.Name == "New Hire Two" && !e.IsActive);
    }

    [Fact]
    public async Task Import_MixedValidAndInvalidRows_CommitsNothingAndReportsEveryFailingRow()
    {
        var bytes = BuildWorkbook([
            ("Valid Person", "Engineering", "55000", "2024-05-01", "Active"),
            ("", "Engineering", "55000", "2024-05-01", "Active"), // missing name
            ("Bad Department", "NoSuchDept", "55000", "2024-05-01", "Active"), // department doesn't exist
            ("Bad Salary", "Engineering", "not-a-number", "2024-05-01", "Active"), // salary not numeric
            ("Bad Date", "Engineering", "55000", "not-a-date", "Active"), // invalid join date
            ("Bad Status", "Engineering", "55000", "2024-05-01", "Maybe"), // invalid status
        ]);

        var response = await _client.PostAsync("/employees/import", ToFileContent(bytes));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<EmployeeImportResult>();
        Assert.NotNull(result);
        Assert.Equal(0, result!.ImportedCount);
        Assert.NotEmpty(result.Errors);

        // one failing row per deliberately-bad row (row 1 is the header, so data starts at row 2)
        Assert.Contains(result.Errors, e => e.Row == 3 && e.Field == "Name");
        Assert.Contains(result.Errors, e => e.Row == 4 && e.Field == "DepartmentId");
        Assert.Contains(result.Errors, e => e.Row == 5 && e.Field == "Salary");
        Assert.Contains(result.Errors, e => e.Row == 6 && e.Field == "JoinDate");
        Assert.Contains(result.Errors, e => e.Row == 7 && e.Field == "Status");

        // nothing committed, including the one valid row in the same file
        var list = await _client.GetFromJsonAsync<PagedResult<EmployeeListItemDto>>($"/employees?name={Uri.EscapeDataString("Valid Person")}");
        Assert.NotNull(list);
        Assert.Empty(list!.Items);
    }

    [Fact]
    public async Task Export_Excel_ReturnsFilteredEmployeesWithStatusAsText()
    {
        var response = await _client.GetAsync("/employees/export?format=excel&departmentId=" + await GetEngineeringId());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bytes = await response.Content.ReadAsByteArrayAsync();

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var sheet = workbook.Worksheets.First();
        var headerRow = sheet.Row(1).CellsUsed().Select(c => c.GetString()).ToList();
        Assert.Contains("Status", headerRow);

        var statusCol = headerRow.IndexOf("Status") + 1;
        var statuses = sheet.RowsUsed().Skip(1).Select(r => r.Cell(statusCol).GetString()).ToList();
        Assert.NotEmpty(statuses);
        Assert.All(statuses, s => Assert.True(s is "Active" or "In Active"));
    }

    [Fact]
    public async Task Export_Csv_ReturnsFilteredEmployeesWithStatusAsText()
    {
        var response = await _client.GetAsync("/employees/export?format=csv");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var csv = await response.Content.ReadAsStringAsync();

        Assert.Contains("Status", csv);
        Assert.Contains("Active", csv);
        Assert.DoesNotContain("True", csv);
        Assert.DoesNotContain("False", csv);
    }

    [Fact]
    public async Task Export_RoundTrip_ExportedFileReimportsCleanly()
    {
        var exportResponse = await _client.GetAsync($"/employees/export?format=excel&name={Uri.EscapeDataString("Jane Smith")}");
        var exportedBytes = await exportResponse.Content.ReadAsByteArrayAsync();

        var importResponse = await _client.PostAsync("/employees/import", ToFileContent(exportedBytes));

        Assert.Equal(HttpStatusCode.OK, importResponse.StatusCode);
        var result = await importResponse.Content.ReadFromJsonAsync<EmployeeImportResult>();
        Assert.NotNull(result);
        Assert.Equal(1, result!.ImportedCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Viewer_Gets403_OnImportAndExport()
    {
        var viewerClient = factory.CreateClient();
        await AuthTestHelper.LoginAsViewerAsync(viewerClient);

        var exportResponse = await viewerClient.GetAsync("/employees/export?format=csv");
        Assert.Equal(HttpStatusCode.Forbidden, exportResponse.StatusCode);

        var bytes = BuildWorkbook([("X", "Engineering", "1", "2024-01-01", "Active")]);
        var importResponse = await viewerClient.PostAsync("/employees/import", ToFileContent(bytes));
        Assert.Equal(HttpStatusCode.Forbidden, importResponse.StatusCode);
    }

    private async Task<int> GetEngineeringId()
    {
        var departments = await _client.GetFromJsonAsync<List<DepartmentDto>>("/departments");
        return departments!.Single(d => d.Name == "Engineering").Id;
    }
}
