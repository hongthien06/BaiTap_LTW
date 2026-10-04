using Microsoft.EntityFrameworkCore;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Dtos.Public;

namespace NhaGiaKim.Application.Services;

public interface ILandingService
{
    Task<ServiceResult<LandingResponse>> GetAsync(CancellationToken ct = default);
}

/// <summary>Gom toan bo du lieu landing page vao MOT request de tranh waterfall.</summary>
public class LandingService(IAppDbContext db) : ILandingService
{
    public const int MaxFeedbacksOnLanding = 20;

    public async Task<ServiceResult<LandingResponse>> GetAsync(CancellationToken ct = default)
    {
        var book = await db.Books
            .AsNoTracking()
            .Include(b => b.Images)
            .Include(b => b.Author)
            .Include(b => b.PressQuotes)
            .Include(b => b.ContentReview)
            .Where(b => b.IsActive)
            .OrderBy(b => b.Id)
            .FirstOrDefaultAsync(ct);

        if (book is null)
        {
            return ServiceResult<LandingResponse>.Fail(
                ServiceErrorCode.NotFound, "Chua co sach nao duoc kich hoat.");
        }

        // Chi feedback DA DUYET moi duoc hien va duoc tinh diem trung binh (AC-14).
        var approved = db.Feedbacks.AsNoTracking()
            .Where(f => f.BookId == book.Id && f.IsApproved);

        var count = await approved.CountAsync(ct);
        var average = count == 0 ? 0d : await approved.AverageAsync(f => (double)f.Rating, ct);

        var feedbacks = await approved
            .OrderByDescending(f => f.CreatedAt)
            .Take(MaxFeedbacksOnLanding)
            .Select(f => new FeedbackDto(f.Id, f.CustomerName, f.Rating, f.Content, f.CreatedAt))
            .ToListAsync(ct);

        var settings = await db.SiteSettings.AsNoTracking()
            .ToDictionaryAsync(s => s.Key, s => s.Value, ct);

        var dto = new LandingResponse(
            Book: new BookDto(
                book.Id, book.Name, book.Category, book.Title, book.Subtitle, book.Description,
                book.Price, book.DiscountPrice, book.CoverImageUrl, book.MockupImageUrl,
                book.Images.OrderBy(i => i.SortOrder)
                    .Select(i => new BookImageDto(i.Url, i.Caption, i.SortOrder)).ToList()),
            Author: book.Author is null
                ? null
                : new AuthorDto(book.Author.FullName, book.Author.AvatarUrl, book.Author.Bio),
            PressQuotes: book.PressQuotes.OrderBy(p => p.SortOrder)
                .Select(p => new PressQuoteDto(p.PressName, p.LogoUrl, p.Quote, p.SourceUrl)).ToList(),
            Review: book.ContentReview is null
                ? null
                : new ContentReviewDto(book.ContentReview.Title, book.ContentReview.Content, book.ContentReview.FileUrl),
            RatingSummary: new RatingSummaryDto(Math.Round(average, 2), count),
            Feedbacks: feedbacks,
            Settings: settings);

        return ServiceResult<LandingResponse>.Ok(dto);
    }
}
