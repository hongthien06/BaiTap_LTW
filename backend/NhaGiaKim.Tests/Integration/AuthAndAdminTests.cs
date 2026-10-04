using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Application.Dtos.Public;
using NhaGiaKim.Domain.Enums;
using NhaGiaKim.Infrastructure;
using Shouldly;

namespace NhaGiaKim.Tests.Integration;

public class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // AC-17
    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwtWithRole()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(ApiFactory.AdminEmail, ApiFactory.AdminPassword));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(TestHttpExtensions.Json);
        body!.AccessToken.ShouldNotBeNullOrWhiteSpace();
        body.User.Role.ShouldBe(RoleName.Admin);
        body.ExpiresAtUtc.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    // AC-18: sai mat khau va email khong ton tai phai TRA VE CUNG MOT thong bao.
    [Fact]
    public async Task Login_WrongPasswordAndUnknownEmail_ReturnSameGenericError()
    {
        var client = factory.CreateClient();

        var wrongPassword = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(ApiFactory.AdminEmail, "sai-mat-khau"));
        var unknownEmail = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("khongcoai@nhagiakim.local", "sai-mat-khau"));

        wrongPassword.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        unknownEmail.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var a = await wrongPassword.Content.ReadAsStringAsync();
        var b = await unknownEmail.Content.ReadAsStringAsync();
        a.ShouldContain("Email hoac mat khau khong dung.");
        b.ShouldContain("Email hoac mat khau khong dung.");
    }

    // AC-19
    [Fact]
    public async Task AdminEndpoint_WithoutToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/admin/orders");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminEndpoint_WithTamperedToken_Returns401()
    {
        var client = factory.CreateClient();
        var token = await client.LoginAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token + "xyz");
        var response = await client.GetAsync("/api/admin/orders");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // AC-20
    [Fact]
    public async Task StaffToken_UpdateBook_Returns403_ButCanReadOrders()
    {
        var staff = await factory.AuthenticatedAsStaffAsync();

        var forbidden = await staff.PutAsJsonAsync("/api/admin/book", new UpdateBookRequest(
            "x", "x", "x", "x", "x", 1000m, null, null, null, true));
        var allowed = await staff.GetAsync("/api/admin/orders");

        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        allowed.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // AC-22: mat khau admin phai duoc hash BCrypt, khong luu plaintext.
    [Fact]
    public async Task Seed_StoresAdminPasswordAsBcryptHash()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var admin = await db.AppUsers.FirstAsync(u => u.Email == ApiFactory.AdminEmail);

        admin.PasswordHash.ShouldStartWith("$2");                 // dinh danh BCrypt
        admin.PasswordHash.ShouldNotContain(ApiFactory.AdminPassword);
        admin.PasswordHash.Length.ShouldBeGreaterThan(50);
    }
}

public class AdminOrderTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static CreateOrderRequest Order(string phone) =>
        new("Nguyen Van A", phone, "123 Duong Sach, Quan 1, TPHCM", 1, PaymentMethod.Cod, null);

    // AC-24
    [Fact]
    public async Task GetOrders_Pagination_ReturnsPageSizeAndTotalCount()
    {
        var pub = factory.CreateClient();
        for (var i = 0; i < 12; i++)
            (await pub.PostAsJsonAsync("/api/orders", Order("0901234567"))).EnsureSuccessStatusCode();

        var admin = await factory.AuthenticatedAsAdminAsync();
        var page = await admin.GetFromJsonAsync<PagedResult<AdminOrderDto>>(
            "/api/admin/orders?page=1&pageSize=10", TestHttpExtensions.Json);

        page!.Items.Count.ShouldBe(10);
        page.TotalCount.ShouldBeGreaterThanOrEqualTo(12);
        page.Page.ShouldBe(1);
    }

    // AC-25
    [Fact]
    public async Task GetOrders_FilterByStatusAndPhone_AppliesBothConditions()
    {
        var pub = factory.CreateClient();
        (await pub.PostAsJsonAsync("/api/orders", Order("0912345678"))).EnsureSuccessStatusCode();

        var admin = await factory.AuthenticatedAsAdminAsync();
        var page = await admin.GetFromJsonAsync<PagedResult<AdminOrderDto>>(
            "/api/admin/orders?status=New&phone=0912345678", TestHttpExtensions.Json);

        page!.Items.ShouldNotBeEmpty();
        page.Items.ShouldAllBe(o => o.Phone == "0912345678" && o.Status == OrderStatus.New);
    }

    // AC-26 + AC-27
    [Fact]
    public async Task UpdateStatus_FollowsStateMachine()
    {
        var pub = factory.CreateClient();
        var created = await pub.PostAsJsonAsync("/api/orders", Order("0988888888"));
        created.EnsureSuccessStatusCode();

        var admin = await factory.AuthenticatedAsAdminAsync();
        var page = await admin.GetFromJsonAsync<PagedResult<AdminOrderDto>>(
            "/api/admin/orders?phone=0988888888", TestHttpExtensions.Json);
        var id = page!.Items[0].Id;

        var forward = await admin.PatchAsJsonAsync($"/api/admin/orders/{id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Confirmed));
        var backward = await admin.PatchAsJsonAsync($"/api/admin/orders/{id}/status",
            new UpdateOrderStatusRequest(OrderStatus.New));
        var skipping = await admin.PatchAsJsonAsync($"/api/admin/orders/{id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Completed));

        forward.StatusCode.ShouldBe(HttpStatusCode.OK);
        backward.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        skipping.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}

public class AdminContentTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // AC-23
    [Fact]
    public async Task UpdateBook_ReflectsOnLanding()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();
        var current = await admin.GetFromJsonAsync<AdminBookDto>("/api/admin/book", TestHttpExtensions.Json);

        var newTitle = "Nha Gia Kim - Ban ky niem " + Guid.NewGuid().ToString("N")[..6];
        var update = await admin.PutAsJsonAsync("/api/admin/book", new UpdateBookRequest(
            current!.Name, current.Category, newTitle, current.Subtitle, current.Description,
            current.Price, current.DiscountPrice, current.CoverImageUrl, current.MockupImageUrl, true));
        update.EnsureSuccessStatusCode();

        var landing = await factory.CreateClient()
            .GetFromJsonAsync<LandingResponse>("/api/public/landing", TestHttpExtensions.Json);

        landing!.Book.Title.ShouldBe(newTitle);
    }

    [Fact]
    public async Task UpdateBook_DiscountNotLowerThanPrice_Returns400()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();
        var current = await admin.GetFromJsonAsync<AdminBookDto>("/api/admin/book", TestHttpExtensions.Json);

        var response = await admin.PutAsJsonAsync("/api/admin/book", new UpdateBookRequest(
            current!.Name, current.Category, current.Title, current.Subtitle, current.Description,
            100_000m, 100_000m, null, null, true));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // AC-28
    [Fact]
    public async Task ApproveFeedback_MakesItVisibleAndRecalculatesAverage()
    {
        var pub = factory.CreateClient();
        var content = "Danh gia can duyet " + Guid.NewGuid().ToString("N")[..6];
        (await pub.PostAsJsonAsync("/api/feedbacks",
            new CreateFeedbackRequest("Tran Thi B", 5, content))).EnsureSuccessStatusCode();

        var admin = await factory.AuthenticatedAsAdminAsync();
        var pending = await admin.GetFromJsonAsync<PagedResult<AdminFeedbackDto>>(
            "/api/admin/feedbacks?isApproved=false&pageSize=100", TestHttpExtensions.Json);
        var target = pending!.Items.First(f => f.Content == content);

        var approve = await admin.PatchAsync($"/api/admin/feedbacks/{target.Id}/approve", null);
        approve.EnsureSuccessStatusCode();

        var landing = await pub.GetFromJsonAsync<LandingResponse>("/api/public/landing", TestHttpExtensions.Json);
        landing!.Feedbacks.ShouldContain(f => f.Content == content);
        landing.RatingSummary.Count.ShouldBeGreaterThan(0);
    }

    // AC-30
    [Fact]
    public async Task UpdateSetting_ReflectsInLandingSettings()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();
        var hotline = "0909" + Random.Shared.Next(100000, 999999);

        var update = await admin.PutAsJsonAsync("/api/admin/settings", new UpdateSettingsRequest(
            [new SettingItemDto("contact.hotline", hotline, "So hotline")]));
        update.EnsureSuccessStatusCode();

        var landing = await factory.CreateClient()
            .GetFromJsonAsync<LandingResponse>("/api/public/landing", TestHttpExtensions.Json);

        landing!.Settings["contact.hotline"].ShouldBe(hotline);
    }

    // AC-29
    [Fact]
    public async Task Upload_ExecutableDisguisedAsImage_Returns400()
    {
        var admin = await factory.AuthenticatedAsAdminAsync();

        using var content = new MultipartFormDataContent();
        var fake = new ByteArrayContent([0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00]);
        fake.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fake, "file", "anh-dep.jpg");

        var response = await admin.PostAsync("/api/admin/upload?kind=Image", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
