using System.Net.Http.Json;
using ExcelManagement.Application.Auth;

namespace ExcelManagement.Api.IntegrationTests;

public static class AuthTestHelper
{
    public static async Task LoginAsAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest("admin", "admin123"));
        response.EnsureSuccessStatusCode();
    }

    public static async Task LoginAsViewerAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest("viewer", "viewer123"));
        response.EnsureSuccessStatusCode();
    }
}
