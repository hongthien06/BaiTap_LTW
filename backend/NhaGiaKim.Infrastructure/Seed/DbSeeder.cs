using Microsoft.EntityFrameworkCore;
using NhaGiaKim.Domain.Entities;
using NhaGiaKim.Domain.Enums;

namespace NhaGiaKim.Infrastructure.Seed;

public static class DbSeeder
{
    public const string DefaultAdminEmail = "admin@nhagiakim.local";

    /// <summary>
    /// Seed du lieu khoi tao. Idempotent: chay nhieu lan khong tao trung.
    /// Mat khau admin lay tu cau hinh (user-secrets / bien moi truong), KHONG hard-code va KHONG log ra.
    /// </summary>
    public static async Task SeedAsync(AppDbContext db, string defaultAdminPassword, CancellationToken ct = default)
    {
        await SeedRolesAsync(db, ct);
        await SeedAdminAsync(db, defaultAdminPassword, ct);
        await SeedSettingsAsync(db, ct);
        await SeedBookAsync(db, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedRolesAsync(AppDbContext db, CancellationToken ct)
    {
        // Roles da duoc HasData trong RoleConfiguration; day la luoi an toan cho provider khong chay migration.
        if (!await db.Roles.AnyAsync(ct))
        {
            db.Roles.AddRange(
                new Role { Id = 1, Name = RoleName.Admin },
                new Role { Id = 2, Name = RoleName.Staff });
            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task SeedAdminAsync(AppDbContext db, string password, CancellationToken ct)
    {
        if (await db.AppUsers.AnyAsync(ct)) return;

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Thieu mat khau admin mac dinh. Dat Seed:AdminPassword qua user-secrets hoac bien moi truong.");
        }

        var adminRoleId = await db.Roles.Where(r => r.Name == RoleName.Admin)
            .Select(r => r.Id).FirstAsync(ct);

        db.AppUsers.Add(new AppUser
        {
            Email = DefaultAdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            FullName = "Quản trị viên",
            RoleId = adminRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static async Task SeedSettingsAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.SiteSettings.AnyAsync(ct)) return;

        db.SiteSettings.AddRange(
            new SiteSetting { Key = "site.logo", Value = "/img/logo.svg", Description = "Logo hiện ở header và chân trang" },
            new SiteSetting { Key = "contact.hotline", Value = "1900 1234", Description = "Số hotline" },
            new SiteSetting { Key = "contact.email", Value = "lienhe@nhagiakim.local", Description = "Email liên hệ" },
            new SiteSetting { Key = "contact.address", Value = "123 Đường Sách, Quận 1, TP.HCM", Description = "Địa chỉ" },
            new SiteSetting { Key = "social.facebook", Value = "https://facebook.com", Description = "Link Facebook" },
            new SiteSetting { Key = "social.youtube", Value = "https://youtube.com", Description = "Link YouTube" },
            new SiteSetting { Key = "footer.text", Value = "© 2026 Nhà Giả Kim. Bài tập môn Lập trình Web.", Description = "Dòng bản quyền" },
            new SiteSetting { Key = "payment.bankInfo", Value = "Vietcombank · 0123456789 · NGUYEN VAN A", Description = "Thông tin chuyển khoản" });
    }

    private static async Task SeedBookAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.Books.AnyAsync(ct)) return;

        var now = DateTime.UtcNow;
        var book = new Book
        {
            Name = "Nhà Giả Kim",
            Category = "Tiểu thuyết · Truyền cảm hứng",
            Title = "Nhà Giả Kim",
            Subtitle = "Khi bạn khao khát một điều gì đó, cả vũ trụ sẽ hợp lực giúp bạn đạt được điều đó.",
            Description = "Câu chuyện về chàng trai chăn cừu Santiago trên hành trình đi tìm kho báu " +
                          "và khám phá ra ý nghĩa thật sự của vận mệnh cá nhân.",
            Price = 89_000m,
            DiscountPrice = 69_000m,
            CoverImageUrl = "/img/book-cover.svg",
            MockupImageUrl = "/img/book-mockup.svg",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Author = new Author
            {
                FullName = "Paulo Coelho",
                AvatarUrl = "/img/author.svg",
                Bio = "Tiểu thuyết gia người Brazil, tác giả của nhiều đầu sách được dịch ra hơn 80 ngôn ngữ. "
                      + "Nhà Giả Kim là tác phẩm đưa tên tuổi ông ra thế giới."
            },
            ContentReview = new ContentReview
            {
                Title = "Review nội dung",
                Content = "Một cuốn sách ngắn nhưng đọng, phù hợp với người đang đi tìm hướng đi cho chính mình.",
                FileUrl = null
            },
            PressQuotes =
            [
                new PressQuote { PressName = "Tuổi Trẻ", Quote = "Một trong những cuốn sách bán chạy nhất mọi thời đại.", SourceUrl = "https://tuoitre.vn", SortOrder = 1 },
                new PressQuote { PressName = "Thanh Niên", Quote = "Hành trình đi tìm vận mệnh cá nhân được kể lại đầy cuốn hút.", SourceUrl = "https://thanhnien.vn", SortOrder = 2 },
                new PressQuote { PressName = "VnExpress", Quote = "Cuốn sách nên đọc ít nhất một lần trong đời.", SourceUrl = "https://vnexpress.net", SortOrder = 3 }
            ],
            Images =
            [
                new BookImage { Url = "/img/book-1.svg", Caption = "Bìa trước", SortOrder = 1 },
                new BookImage { Url = "/img/book-2.svg", Caption = "Bìa sau", SortOrder = 2 }
            ]
        };

        db.Books.Add(book);
    }
}
