using System.Net;
using System.Net.Http.Json;
using NhaGiaKim.Application.Dtos.Public;
using NhaGiaKim.Domain.Enums;
using Shouldly;

namespace NhaGiaKim.Tests.Integration;

public class PublicApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static CreateOrderRequest ValidOrder(int quantity = 1) =>
        new("Nguyen Van A", "0901234567", "123 Duong Sach, Quan 1, TPHCM", quantity, PaymentMethod.Cod, null);

    // AC-1
    [Fact]
    public async Task GetLanding_ReturnsFullAggregate()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/public/landing");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LandingResponse>(TestHttpExtensions.Json);

        body.ShouldNotBeNull();
        body.Book.Name.ShouldNotBeNullOrWhiteSpace();
        body.Author.ShouldNotBeNull();
        body.PressQuotes.ShouldNotBeEmpty();
        body.Review.ShouldNotBeNull();
        body.Settings.Keys.ShouldContain("contact.hotline");
        body.RatingSummary.ShouldNotBeNull();
    }

    // AC-7
    [Fact]
    public async Task CreateOrder_ValidPayload_Returns201WithCode()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", ValidOrder());

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateOrderResponse>(TestHttpExtensions.Json);
        body!.OrderCode.ShouldStartWith("NGK-");
        body.OrderCode.Length.ShouldBe(17);   // NGK- + yyyyMMdd + - + 4 ky tu
    }

    // AC-10: server bo qua moi gia tri gia do client gui len.
    [Fact]
    public async Task CreateOrder_IgnoresClientSuppliedTotal()
    {
        var client = factory.CreateClient();

        var payload = new
        {
            customerName = "Nguyen Van A",
            phone = "0901234567",
            address = "123 Duong Sach, Quan 1, TPHCM",
            quantity = 2,
            paymentMethod = 0,
            unitPrice = 1,      // gia bia dat
            totalPrice = 2      // tong bia dat
        };

        var response = await client.PostAsJsonAsync("/api/orders", payload);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateOrderResponse>(TestHttpExtensions.Json);
        body!.TotalPrice.ShouldBe(body.UnitPrice * 2);
        body.UnitPrice.ShouldBeGreaterThan(1m);
    }

    // AC-8
    [Fact]
    public async Task CreateOrder_InvalidPhone_Returns400()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", ValidOrder() with { Phone = "901234567" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Phone");
    }

    // AC-9
    [Theory]
    [InlineData(0, HttpStatusCode.BadRequest)]
    [InlineData(100, HttpStatusCode.BadRequest)]
    [InlineData(1, HttpStatusCode.Created)]
    [InlineData(99, HttpStatusCode.Created)]
    public async Task CreateOrder_QuantityBoundaries(int quantity, HttpStatusCode expected)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", ValidOrder(quantity));

        response.StatusCode.ShouldBe(expected);
    }

    // AC-13
    [Fact]
    public async Task CreateFeedback_DefaultsToUnapproved()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/feedbacks",
            new CreateFeedbackRequest("Tran Thi B", 5, "Sach rat hay"));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        // AC-14: feedback vua tao chua duoc duyet nen khong duoc xuat hien tren landing.
        var landing = await client.GetFromJsonAsync<LandingResponse>("/api/public/landing", TestHttpExtensions.Json);
        landing!.Feedbacks.ShouldNotContain(f => f.Content == "Sach rat hay");
    }

    // AC-15
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task CreateFeedback_RatingOutOfRange_Returns400(int rating)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/feedbacks",
            new CreateFeedbackRequest("Tran Thi B", rating, "Noi dung"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}

/// <summary>Tach rieng vi can gioi han rate limit that chat, khong lam nhieu test khac.</summary>
public class RateLimitTests(RateLimitedApiFactory factory) : IClassFixture<RateLimitedApiFactory>
{
    // AC-11
    [Fact]
    public async Task CreateOrder_SixthRequestInWindow_Returns429()
    {
        var client = factory.CreateClient();
        var payload = new CreateOrderRequest(
            "Nguyen Van A", "0901234567", "123 Duong Sach, Quan 1, TPHCM", 1, PaymentMethod.Cod, null);

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 6; i++)
        {
            var response = await client.PostAsJsonAsync("/api/orders", payload);
            statuses.Add(response.StatusCode);
        }

        statuses.Take(5).ShouldAllBe(s => s == HttpStatusCode.Created);
        statuses[5].ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
