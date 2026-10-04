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
            FullName = "Quan tri vien",
            RoleId = adminRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static async Task SeedSettingsAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.SiteSettings.AnyAsync(ct)) return;

        db.SiteSettings.AddRange(
            new SiteSetting { Key = "site.logo", Value = "/img/logo.svg", Description = "Logo hien o header/footer" },
            new SiteSetting { Key = "contact.hotline", Value = "1900 1234", Description = "So hotline" },
            new SiteSetting { Key = "contact.email", Value = "lienhe@nhagiakim.local", Description = "Email lien he" },
            new SiteSetting { Key = "contact.address", Value = "123 Duong Sach, Quan 1, TP.HCM", Description = "Dia chi" },
            new SiteSetting { Key = "social.facebook", Value = "https://facebook.com", Description = "Link Facebook" },
            new SiteSetting { Key = "social.youtube", Value = "https://youtube.com", Description = "Link YouTube" },
            new SiteSetting { Key = "footer.text", Value = "(c) 2026 Nha Gia Kim. Bai tap mon Lap trinh Web.", Description = "Dong ban quyen" },
            new SiteSetting { Key = "payment.bankInfo", Value = "Vietcombank - 0123456789 - NGUYEN VAN A", Description = "Thong tin chuyen khoan" });
    }

    private static async Task SeedBookAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.Books.AnyAsync(ct)) return;

        var now = DateTime.UtcNow;
        var book = new Book
        {
            Name = "Nha Gia Kim",
            Category = "Tieu thuyet / Truyen cam hung",
            Title = "Nha Gia Kim",
            Subtitle = "Khi ban khao khat mot dieu gi do, ca vu tru se hop luc giup ban dat duoc dieu do.",
            Description = "Cau chuyen ve chang trai chan cuu Santiago tren hanh trinh di tim kho bau " +
                          "va kham pha ra y nghia that su cua van menh ca nhan.",
            Price = 89_000m,
            DiscountPrice = 69_000m,
            CoverImageUrl = "/img/book-cover.jpg",
            MockupImageUrl = "/img/book-mockup.png",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Author = new Author
            {
                FullName = "Paulo Coelho",
                AvatarUrl = "/img/author.jpg",
                Bio = "Tieu thuyet gia nguoi Brazil, tac gia cua nhieu dau sach duoc dich ra hon 80 ngon ngu."
            },
            ContentReview = new ContentReview
            {
                Title = "Review noi dung",
                Content = "Mot cuon sach ngan nhung dat, phu hop voi nguoi dang di tim huong di cho chinh minh.",
                FileUrl = null
            },
            PressQuotes =
            [
                new PressQuote { PressName = "Tuoi Tre", Quote = "Mot trong nhung cuon sach ban chay nhat moi thoi dai.", SourceUrl = "https://tuoitre.vn", SortOrder = 1 },
                new PressQuote { PressName = "Thanh Nien", Quote = "Hanh trinh di tim van menh ca nhan duoc ke lai day cuon hut.", SourceUrl = "https://thanhnien.vn", SortOrder = 2 },
                new PressQuote { PressName = "VnExpress", Quote = "Cuon sach nen doc it nhat mot lan trong doi.", SourceUrl = "https://vnexpress.net", SortOrder = 3 }
            ],
            Images =
            [
                new BookImage { Url = "/img/book-1.jpg", Caption = "Bia truoc", SortOrder = 1 },
                new BookImage { Url = "/img/book-2.jpg", Caption = "Bia sau", SortOrder = 2 }
            ]
        };

        db.Books.Add(book);
    }
}
