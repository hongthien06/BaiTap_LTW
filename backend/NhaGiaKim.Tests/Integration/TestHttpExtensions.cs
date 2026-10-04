using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NhaGiaKim.Application.Dtos.Admin;

namespace NhaGiaKim.Tests.Integration;

public static class TestHttpExtensions
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<string> LoginAsync(this HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(Json);
        return body!.AccessToken;
    }

    public static async Task<HttpClient> AuthenticatedAsAdminAsync(this ApiFactory factory)
    {
        var client = factory.CreateClient();
        var token = await client.LoginAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<HttpClient> AuthenticatedAsStaffAsync(this ApiFactory factory)
    {
        const string email = "staff@nhagiakim.local";
        const string password = "Staff@12345";

        var admin = await factory.AuthenticatedAsAdminAsync();
        var created = await admin.PostAsJsonAsync("/api/admin/users",
            new CreateUserRequest(email, password, "Nhan vien", "Staff"));
        created.EnsureSuccessStatusCode();

        var client = factory.CreateClient();
        var token = await client.LoginAsync(email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
