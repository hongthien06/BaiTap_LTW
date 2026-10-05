using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Application.Dtos.Public;
using NhaGiaKim.Domain.Enums;
using NhaGiaKim.Infrastructure;
using Shouldly;

namespace NhaGiaKim.Tests.Integration;

/// <summary>
/// Hoi quy cho mot loi that: don vua dat xong da bi gan nhan "Qua han xu ly".
///
/// Nguyen nhan: cot datetime2 cua SQL Server khong luu mui gio nen EF tra ve
/// DateTimeKind.Unspecified; System.Text.Json ghi ra chuoi khong co hau to "Z";
/// trinh duyet hieu chuoi do la gio dia phuong. May o UTC+7 thi moc thoi gian bi
/// lui 7 tieng, vuot nguong "qua han 4 gio" ngay lap tuc.
/// </summary>
public class UtcTimestampTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task CreatedAt_DocTuDatabase_PhaiMangKindUtc()
    {
        var pub = factory.CreateClient();
        (await pub.PostAsJsonAsync("/api/orders", new CreateOrderRequest(
            "Kiem Tra UTC", "0901111222", "1 Duong Test, Quan 1, TPHCM", 1, PaymentMethod.Cod, null)))
            .EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var order = await db.Orders.AsNoTracking().OrderByDescending(o => o.Id).FirstAsync();

        order.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        order.UpdatedAt.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public async Task CreatedAt_TrongJson_PhaiCoHauToZ()
    {
        var pub = factory.CreateClient();
        (await pub.PostAsJsonAsync("/api/orders", new CreateOrderRequest(
            "Kiem Tra Z", "0902222333", "2 Duong Test, Quan 1, TPHCM", 1, PaymentMethod.Cod, null)))
            .EnsureSuccessStatusCode();

        var admin = await factory.AuthenticatedAsAdminAsync();
        var raw = await admin.GetStringAsync("/api/admin/orders?page=1&pageSize=1");

        using var doc = JsonDocument.Parse(raw);
        var createdAt = doc.RootElement.GetProperty("items")[0].GetProperty("createdAt").GetString();

        createdAt.ShouldNotBeNull();
        // Khong co "Z" thi trinh duyet se hieu nham la gio dia phuong.
        createdAt.ShouldEndWith("Z");
    }

    [Fact]
    public async Task DonVuaDat_KhongDuocCoiLaQuaHan()
    {
        var pub = factory.CreateClient();
        (await pub.PostAsJsonAsync("/api/orders", new CreateOrderRequest(
            "Kiem Tra Qua Han", "0903333444", "3 Duong Test, Quan 1, TPHCM", 1, PaymentMethod.Cod, null)))
            .EnsureSuccessStatusCode();

        var admin = await factory.AuthenticatedAsAdminAsync();
        var page = await admin.GetFromJsonAsync<PagedResult<AdminOrderDto>>(
            "/api/admin/orders?page=1&pageSize=1", TestHttpExtensions.Json);

        var order = page!.Items[0];

        // Day la dung phep tinh ma giao dien dung de quyet dinh "qua han xu ly".
        var tuoiDon = DateTime.UtcNow - order.CreatedAt.ToUniversalTime();

        tuoiDon.ShouldBeLessThan(TimeSpan.FromMinutes(5));
        tuoiDon.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
    }
}
