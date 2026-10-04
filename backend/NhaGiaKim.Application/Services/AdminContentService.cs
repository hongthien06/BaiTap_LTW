using Microsoft.EntityFrameworkCore;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Common;
using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Domain.Entities;

namespace NhaGiaKim.Application.Services;

public interface IAdminContentService
{
    Task<ServiceResult<AdminBookDto>> GetBookAsync(CancellationToken ct = default);
    Task<ServiceResult<AdminBookDto>> UpdateBookAsync(UpdateBookRequest req, CancellationToken ct = default);
    Task<ServiceResult<AdminAuthorDto>> GetAuthorAsync(CancellationToken ct = default);
    Task<ServiceResult<AdminAuthorDto>> UpsertAuthorAsync(UpdateAuthorRequest req, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<AdminPressQuoteDto>>> GetPressQuotesAsync(CancellationToken ct = default);
    Task<ServiceResult<AdminPressQuoteDto>> CreatePressQuoteAsync(PressQuoteRequest req, CancellationToken ct = default);
    Task<ServiceResult<AdminPressQuoteDto>> UpdatePressQuoteAsync(int id, PressQuoteRequest req, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeletePressQuoteAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<AdminReviewDto>> GetReviewAsync(CancellationToken ct = default);
    Task<ServiceResult<AdminReviewDto>> UpsertReviewAsync(UpdateReviewRequest req, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<SettingItemDto>>> GetSettingsAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<SettingItemDto>>> UpdateSettingsAsync(UpdateSettingsRequest req, CancellationToken ct = default);
}

public class AdminContentService(IAppDbContext db, TimeProvider clock) : IAdminContentService
{
    /// <summary>
    /// Chi nhung key nay duoc phep ton tai. Khong whitelist thi admin co the tao key bat ky
    /// (mat khau SMTP, API key...) va LandingService se do het ra endpoint cong khai.
    /// </summary>
    private static readonly string[] AllowedSettingKeys = PublicSettingKeys.All;

    /// <summary>
    /// Phai loc IsActive giong het LandingService va OrderService. Neu khong, khi co nhieu hon
    /// mot ban ghi Book, admin se sua quyen dau tien trong bang con landing lai doc quyen active
    /// dau tien - sua xong khong thay gi doi (AC-23 am tham fail).
    /// </summary>
    private Task<Book?> ActiveBookAsync(CancellationToken ct) =>
        db.Books.Where(b => b.IsActive).OrderBy(b => b.Id).FirstOrDefaultAsync(ct);

    private static AdminBookDto ToDto(Book b) => new(
        b.Id, b.Name, b.Category, b.Title, b.Subtitle, b.Description,
        b.Price, b.DiscountPrice, b.CoverImageUrl, b.MockupImageUrl, b.IsActive, b.UpdatedAt);

    public async Task<ServiceResult<AdminBookDto>> GetBookAsync(CancellationToken ct = default)
    {
        var book = await ActiveBookAsync(ct);
        return book is null
            ? ServiceResult<AdminBookDto>.Fail(ServiceErrorCode.NotFound, "Chua co sach.")
            : ServiceResult<AdminBookDto>.Ok(ToDto(book));
    }

    public async Task<ServiceResult<AdminBookDto>> UpdateBookAsync(UpdateBookRequest req, CancellationToken ct = default)
    {
        var book = await ActiveBookAsync(ct);
        if (book is null) return ServiceResult<AdminBookDto>.Fail(ServiceErrorCode.NotFound, "Chua co sach.");

        if (req.Price <= 0)
            return ServiceResult<AdminBookDto>.Invalid(nameof(req.Price), "Gia phai lon hon 0.");
        if (req.DiscountPrice.HasValue && (req.DiscountPrice.Value <= 0 || req.DiscountPrice.Value >= req.Price))
            return ServiceResult<AdminBookDto>.Invalid(nameof(req.DiscountPrice), "Gia giam phai lon hon 0 va nho hon gia goc.");

        // Tat quyen sach cuoi cung se lam GET /api/public/landing tra 404 va landing page chet.
        // Chan truoc thay vi de admin sap site bang mot cu gat.
        if (!req.IsActive && !await db.Books.AnyAsync(b => b.Id != book.Id && b.IsActive, ct))
            return ServiceResult<AdminBookDto>.Invalid(nameof(req.IsActive), "Phai con it nhat mot sach dang mo ban.");

        book.Name = req.Name.Trim();
        book.Category = req.Category.Trim();
        book.Title = req.Title.Trim();
        book.Subtitle = req.Subtitle.Trim();
        book.Description = req.Description;
        book.Price = req.Price;
        book.DiscountPrice = req.DiscountPrice;
        book.CoverImageUrl = req.CoverImageUrl;
        book.MockupImageUrl = req.MockupImageUrl;
        book.IsActive = req.IsActive;
        book.UpdatedAt = clock.GetUtcNow().UtcDateTime;

        await db.SaveChangesAsync(ct);
        return ServiceResult<AdminBookDto>.Ok(ToDto(book));
    }

    public async Task<ServiceResult<AdminAuthorDto>> GetAuthorAsync(CancellationToken ct = default)
    {
        var author = await db.Authors.AsNoTracking().OrderBy(a => a.Id).FirstOrDefaultAsync(ct);
        return author is null
            ? ServiceResult<AdminAuthorDto>.Fail(ServiceErrorCode.NotFound, "Chua co thong tin tac gia.")
            : ServiceResult<AdminAuthorDto>.Ok(new AdminAuthorDto(author.Id, author.FullName, author.AvatarUrl, author.Bio));
    }

    public async Task<ServiceResult<AdminAuthorDto>> UpsertAuthorAsync(UpdateAuthorRequest req, CancellationToken ct = default)
    {
        var book = await ActiveBookAsync(ct);
        if (book is null) return ServiceResult<AdminAuthorDto>.Fail(ServiceErrorCode.NotFound, "Chua co sach.");

        var author = await db.Authors.FirstOrDefaultAsync(a => a.BookId == book.Id, ct);
        if (author is null)
        {
            author = new Author { BookId = book.Id };
            db.Authors.Add(author);
        }

        author.FullName = req.FullName.Trim();
        author.AvatarUrl = req.AvatarUrl;
        author.Bio = req.Bio;

        await db.SaveChangesAsync(ct);
        return ServiceResult<AdminAuthorDto>.Ok(new AdminAuthorDto(author.Id, author.FullName, author.AvatarUrl, author.Bio));
    }

    public async Task<ServiceResult<IReadOnlyList<AdminPressQuoteDto>>> GetPressQuotesAsync(CancellationToken ct = default)
    {
        var items = await db.PressQuotes.AsNoTracking()
            .OrderBy(p => p.SortOrder)
            .Select(p => new AdminPressQuoteDto(p.Id, p.PressName, p.LogoUrl, p.Quote, p.SourceUrl, p.SortOrder))
            .ToListAsync(ct);
        return ServiceResult<IReadOnlyList<AdminPressQuoteDto>>.Ok(items);
    }

    public async Task<ServiceResult<AdminPressQuoteDto>> CreatePressQuoteAsync(PressQuoteRequest req, CancellationToken ct = default)
    {
        var book = await ActiveBookAsync(ct);
        if (book is null) return ServiceResult<AdminPressQuoteDto>.Fail(ServiceErrorCode.NotFound, "Chua co sach.");

        var entity = new PressQuote
        {
            BookId = book.Id,
            PressName = req.PressName.Trim(),
            LogoUrl = req.LogoUrl,
            Quote = req.Quote.Trim(),
            SourceUrl = req.SourceUrl,
            SortOrder = req.SortOrder
        };
        db.PressQuotes.Add(entity);
        await db.SaveChangesAsync(ct);

        return ServiceResult<AdminPressQuoteDto>.Ok(new AdminPressQuoteDto(
            entity.Id, entity.PressName, entity.LogoUrl, entity.Quote, entity.SourceUrl, entity.SortOrder));
    }

    public async Task<ServiceResult<AdminPressQuoteDto>> UpdatePressQuoteAsync(int id, PressQuoteRequest req, CancellationToken ct = default)
    {
        var entity = await db.PressQuotes.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null) return ServiceResult<AdminPressQuoteDto>.Fail(ServiceErrorCode.NotFound, "Khong tim thay trich dan.");

        entity.PressName = req.PressName.Trim();
        entity.LogoUrl = req.LogoUrl;
        entity.Quote = req.Quote.Trim();
        entity.SourceUrl = req.SourceUrl;
        entity.SortOrder = req.SortOrder;
        await db.SaveChangesAsync(ct);

        return ServiceResult<AdminPressQuoteDto>.Ok(new AdminPressQuoteDto(
            entity.Id, entity.PressName, entity.LogoUrl, entity.Quote, entity.SourceUrl, entity.SortOrder));
    }

    public async Task<ServiceResult<bool>> DeletePressQuoteAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.PressQuotes.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null) return ServiceResult<bool>.Fail(ServiceErrorCode.NotFound, "Khong tim thay trich dan.");

        db.PressQuotes.Remove(entity);
        await db.SaveChangesAsync(ct);
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<AdminReviewDto>> GetReviewAsync(CancellationToken ct = default)
    {
        var r = await db.ContentReviews.AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync(ct);
        return r is null
            ? ServiceResult<AdminReviewDto>.Fail(ServiceErrorCode.NotFound, "Chua co review.")
            : ServiceResult<AdminReviewDto>.Ok(new AdminReviewDto(r.Id, r.Title, r.Content, r.FileUrl));
    }

    public async Task<ServiceResult<AdminReviewDto>> UpsertReviewAsync(UpdateReviewRequest req, CancellationToken ct = default)
    {
        var book = await ActiveBookAsync(ct);
        if (book is null) return ServiceResult<AdminReviewDto>.Fail(ServiceErrorCode.NotFound, "Chua co sach.");

        var r = await db.ContentReviews.FirstOrDefaultAsync(x => x.BookId == book.Id, ct);
        if (r is null)
        {
            r = new ContentReview { BookId = book.Id };
            db.ContentReviews.Add(r);
        }

        r.Title = req.Title.Trim();
        r.Content = req.Content;
        r.FileUrl = req.FileUrl;
        await db.SaveChangesAsync(ct);

        return ServiceResult<AdminReviewDto>.Ok(new AdminReviewDto(r.Id, r.Title, r.Content, r.FileUrl));
    }

    public async Task<ServiceResult<IReadOnlyList<SettingItemDto>>> GetSettingsAsync(CancellationToken ct = default)
    {
        var items = await db.SiteSettings.AsNoTracking()
            .OrderBy(s => s.Key)
            .Select(s => new SettingItemDto(s.Key, s.Value, s.Description))
            .ToListAsync(ct);
        return ServiceResult<IReadOnlyList<SettingItemDto>>.Ok(items);
    }

    public async Task<ServiceResult<IReadOnlyList<SettingItemDto>>> UpdateSettingsAsync(UpdateSettingsRequest req, CancellationToken ct = default)
    {
        var unknownKeys = req.Items
            .Select(i => i.Key.Trim())
            .Where(k => !AllowedSettingKeys.Contains(k))
            .ToList();

        if (unknownKeys.Count > 0)
        {
            return ServiceResult<IReadOnlyList<SettingItemDto>>.Invalid(
                "key", $"Key khong duoc phep: {string.Join(", ", unknownKeys)}");
        }

        foreach (var item in req.Items)
        {
            var key = item.Key.Trim();
            var existing = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
            if (existing is null)
            {
                db.SiteSettings.Add(new SiteSetting { Key = key, Value = item.Value, Description = item.Description });
            }
            else
            {
                existing.Value = item.Value;
                existing.Description = item.Description ?? existing.Description;
            }
        }

        await db.SaveChangesAsync(ct);
        return await GetSettingsAsync(ct);
    }
}
