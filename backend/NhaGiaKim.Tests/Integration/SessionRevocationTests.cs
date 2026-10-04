using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NhaGiaKim.Application.Dtos.Admin;
using Shouldly;

namespace NhaGiaKim.Tests.Integration;

/// <summary>
/// Finding F-1 tu buoc 8 (Challenge): khoa tai khoan hoac ha quyen phai co tac dung NGAY,
/// khong doi token het han 60 phut.
/// </summary>
public class SessionRevocationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Password = "Revoke@12345";

    private async Task<(HttpClient Admin, HttpClient Victim, int VictimId)> ArrangeAsync(string role)
    {
        var admin = await factory.AuthenticatedAsAdminAsync();

        // Email duy nhat cho tung test: ca class dung chung mot database (IClassFixture),
        // dung email co dinh thi test thu hai se bi tu choi vi trung email.
        var email = $"revoke-{Guid.NewGuid():N}@nhagiakim.local";

        var created = await admin.PostAsJsonAsync("/api/admin/users",
            new CreateUserRequest(email, Password, "Nan nhan", role));
        created.EnsureSuccessStatusCode();
        var user = await created.Content.ReadFromJsonAsync<AdminUserDto>(TestHttpExtensions.Json);

        var victim = factory.CreateClient();
        var token = await victim.LoginAsync(email, Password);
        victim.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return (admin, victim, user!.Id);
    }

    [Fact]
    public async Task DeactivatedUser_ExistingToken_IsRejectedImmediately()
    {
        var (admin, victim, victimId) = await ArrangeAsync("Staff");

        // Token con hieu luc truoc khi khoa.
        (await victim.GetAsync("/api/admin/orders")).StatusCode.ShouldBe(HttpStatusCode.OK);

        var locked = await admin.PutAsJsonAsync($"/api/admin/users/{victimId}",
            new UpdateUserRequest("Nan nhan", "Staff", IsActive: false, NewPassword: null));
        locked.EnsureSuccessStatusCode();

        // Cung token do, ngay sau khi bi khoa -> phai 401, khong duoc doi het han.
        var after = await victim.GetAsync("/api/admin/orders");
        after.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DemotedUser_ExistingAdminToken_LosesAdminAccessImmediately()
    {
        var (admin, victim, victimId) = await ArrangeAsync("Admin");

        // Dang la Admin nen sua duoc sach.
        var book = await victim.GetAsync("/api/admin/book");
        book.StatusCode.ShouldBe(HttpStatusCode.OK);

        var demoted = await admin.PutAsJsonAsync($"/api/admin/users/{victimId}",
            new UpdateUserRequest("Nan nhan", "Staff", IsActive: true, NewPassword: null));
        demoted.EnsureSuccessStatusCode();

        // Token cu van ghi role=Admin -> phai bi tu choi, khong duoc tin claim trong token.
        var after = await victim.GetAsync("/api/admin/book");
        after.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActiveUser_TokenKeepsWorking()
    {
        var (_, victim, _) = await ArrangeAsync("Staff");

        // Khong doi gi thi phien van phai dung binh thuong qua nhieu request.
        for (var i = 0; i < 3; i++)
        {
            (await victim.GetAsync("/api/admin/orders")).StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}
