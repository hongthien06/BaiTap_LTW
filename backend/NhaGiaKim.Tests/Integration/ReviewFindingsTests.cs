using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Application.Dtos.Public;
using NhaGiaKim.Domain.Entities;
using NhaGiaKim.Domain.Enums;
using NhaGiaKim.Infrastructure;
using Shouldly;

namespace NhaGiaKim.Tests.Integration;

/// <summary>
/// Test hoi quy cho cac finding cua buoc 7 (Code Review) va buoc 8 (Challenge).
/// Moi test o day tung FAIL truoc khi sua - do la ly do no ton tai.
/// </summary>
public class ReviewFindingsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // ---- M5: thieu validator cho DTO admin -> NullReferenceException -> 500 thay vi 400 ----

    [Fact]
    public async Task UpdateBook_EmptyBody_Returns400_NotServerError()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();

        var response = await admin.PutAsJsonAsync("/api/admin/book", new { });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateBook_NameTooLong_Returns400_NotDbError()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();
        var current = await admin.GetFromJsonAsync<AdminBookDto>("/api/admin/book", TestHttpExtensions.Json);

        var response = await admin.PutAsJsonAsync("/api/admin/book", new UpdateBookRequest(
            new string('x', 2000), current!.Category, current.Title, current.Subtitle,
            current.Description, current.Price, current.DiscountPrice, null, null, true));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_InvalidEmail_Returns400()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();

        var response = await admin.PostAsJsonAsync("/api/admin/users",
            new CreateUserRequest("khong-phai-email", "Password@123", "Ten", "Staff"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePressQuote_JavascriptUrl_Returns400()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();

        var response = await admin.PostAsJsonAsync("/api/admin/press-quotes",
            new PressQuoteRequest("Bao Test", null, "Trich dan", "javascript:alert(1)", 1));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // ---- M9: SiteSettings do ra endpoint cong khai khong whitelist ----

    [Fact]
    public async Task UpdateSettings_UnknownKey_Returns400()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();

        var response = await admin.PutAsJsonAsync("/api/admin/settings", new UpdateSettingsRequest(
            [new SettingItemDto("smtp.password", "sieu-bi-mat", null)]));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Landing_NeverExposesNonWhitelistedSetting()
    {
        // Ghi thang vao DB de mo phong du lieu cu da ton tai truoc khi co whitelist.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.SiteSettings.Add(new SiteSetting { Key = "internal.apiKey", Value = "sk-bi-mat" });
            await db.SaveChangesAsync();
        }

        var landing = await factory.CreateClient()
            .GetFromJsonAsync<LandingResponse>("/api/public/landing", TestHttpExtensions.Json);

        landing!.Settings.Keys.ShouldNotContain("internal.apiKey");
        landing.Settings.Keys.ShouldContain("contact.hotline");
    }

    // ---- M8: tat quyen sach cuoi cung lam landing chet ----

    [Fact]
    public async Task UpdateBook_DeactivatingLastActiveBook_Returns400()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();
        var current = await admin.GetFromJsonAsync<AdminBookDto>("/api/admin/book", TestHttpExtensions.Json);

        var response = await admin.PutAsJsonAsync("/api/admin/book", new UpdateBookRequest(
            current!.Name, current.Category, current.Title, current.Subtitle, current.Description,
            current.Price, current.DiscountPrice, current.CoverImageUrl, current.MockupImageUrl,
            IsActive: false));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Landing phai van song.
        (await factory.CreateClient().GetAsync("/api/public/landing"))
            .StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // ---- m26: Staff khong duoc xoa feedback ----

    [Fact]
    public async Task StaffToken_DeleteFeedback_Returns403()
    {
        var pub = factory.CreateClient();
        (await pub.PostAsJsonAsync("/api/feedbacks",
            new CreateFeedbackRequest("Khach", 4, "Noi dung"))).EnsureSuccessStatusCode();

        var admin = await factory.AuthenticatedAsAdminAsync();
        var list = await admin.GetFromJsonAsync<PagedResult<AdminFeedbackDto>>(
            "/api/admin/feedbacks?pageSize=100", TestHttpExtensions.Json);
        var id = list!.Items[0].Id;

        var staff = await factory.AuthenticatedAsStaffAsync();
        var response = await staff.DeleteAsync($"/api/admin/feedbacks/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    // ---- m26: AC-8 yeu cau "khong tao ban ghi nao" ----

    [Fact]
    public async Task CreateOrder_InvalidPhone_WritesNothingToDatabase()
    {
        int Count()
        {
            using var scope = factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<AppDbContext>().Orders.Count();
        }

        var before = Count();

        var response = await factory.CreateClient().PostAsJsonAsync("/api/orders",
            new CreateOrderRequest("Nguyen Van A", "901234567", "123 Duong Sach, Quan 1, TPHCM",
                1, PaymentMethod.Cod, null));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        Count().ShouldBe(before);
    }
}

/// <summary>
/// M10: luat "phai con it nhat mot tai khoan Admin dang hoat dong".
/// Dung factory rieng vi cac test nay thay doi tap Admin cua he thong.
/// </summary>
public class LastAdminRuleTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private async Task<int> AdminIdAsync(HttpClient admin)
    {
        var users = await admin.GetFromJsonAsync<List<AdminUserDto>>("/api/admin/users", TestHttpExtensions.Json);
        return users!.First(u => u.Email == ApiFactory.AdminEmail).Id;
    }

    [Fact]
    public async Task DemotingTheOnlyAdmin_IsRejected()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();
        var id = await AdminIdAsync(admin);

        var response = await admin.PutAsJsonAsync($"/api/admin/users/{id}",
            new UpdateUserRequest("Quan tri vien", "Staff", IsActive: true, NewPassword: null));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Va quan trong nhat: thay doi phai bi rollback, khong duoc ghi nua voi.
        var after = await admin.GetFromJsonAsync<List<AdminUserDto>>("/api/admin/users", TestHttpExtensions.Json);
        after.ShouldNotBeNull();
        after.First(u => u.Id == id).Role.ShouldBe(RoleName.Admin);
        after.ShouldContain(u => u.Role == RoleName.Admin && u.IsActive);
    }

    [Fact]
    public async Task DeactivatingTheOnlyAdmin_IsRejected()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();
        var id = await AdminIdAsync(admin);

        var response = await admin.PutAsJsonAsync($"/api/admin/users/{id}",
            new UpdateUserRequest("Quan tri vien", RoleName.Admin, IsActive: false, NewPassword: null));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var after = await admin.GetFromJsonAsync<List<AdminUserDto>>("/api/admin/users", TestHttpExtensions.Json);
        after.ShouldNotBeNull();
        after.First(u => u.Id == id).IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task DemotingOneOfTwoAdmins_IsAllowed()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();

        var second = await admin.PostAsJsonAsync("/api/admin/users",
            new CreateUserRequest($"admin2-{Guid.NewGuid():N}@nhagiakim.local", "Admin2@12345", "Admin hai", RoleName.Admin));
        second.EnsureSuccessStatusCode();
        var created = await second.Content.ReadFromJsonAsync<AdminUserDto>(TestHttpExtensions.Json);

        var response = await admin.PutAsJsonAsync($"/api/admin/users/{created!.Id}",
            new UpdateUserRequest("Admin hai", "Staff", IsActive: true, NewPassword: null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
