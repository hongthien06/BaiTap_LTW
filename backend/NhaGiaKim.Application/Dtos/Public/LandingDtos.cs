namespace NhaGiaKim.Application.Dtos.Public;

public record LandingResponse(
    BookDto Book,
    AuthorDto? Author,
    IReadOnlyList<PressQuoteDto> PressQuotes,
    ContentReviewDto? Review,
    RatingSummaryDto RatingSummary,
    IReadOnlyList<FeedbackDto> Feedbacks,
    IReadOnlyDictionary<string, string> Settings);

public record BookDto(
    int Id,
    string Name,
    string Category,
    string Title,
    string Subtitle,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    string? CoverImageUrl,
    string? MockupImageUrl,
    IReadOnlyList<BookImageDto> Images);

public record BookImageDto(string Url, string? Caption, int SortOrder);

public record AuthorDto(string FullName, string? AvatarUrl, string Bio);

public record PressQuoteDto(string PressName, string? LogoUrl, string Quote, string? SourceUrl);

public record ContentReviewDto(string Title, string Content, string? FileUrl);

/// <summary>Diem trung binh chi tinh tren feedback DA DUYET.</summary>
public record RatingSummaryDto(double Average, int Count);

public record FeedbackDto(int Id, string CustomerName, int Rating, string Content, DateTime CreatedAt);
