namespace NhaGiaKim.Application.Dtos.Admin;

public record UpdateBookRequest(
    string Name, string Category, string Title, string Subtitle, string Description,
    decimal Price, decimal? DiscountPrice, string? CoverImageUrl, string? MockupImageUrl, bool IsActive);

public record AdminBookDto(
    int Id, string Name, string Category, string Title, string Subtitle, string Description,
    decimal Price, decimal? DiscountPrice, string? CoverImageUrl, string? MockupImageUrl,
    bool IsActive, DateTime UpdatedAt);

public record UpdateAuthorRequest(string FullName, string? AvatarUrl, string Bio);
public record AdminAuthorDto(int Id, string FullName, string? AvatarUrl, string Bio);

public record PressQuoteRequest(string PressName, string? LogoUrl, string Quote, string? SourceUrl, int SortOrder);
public record AdminPressQuoteDto(int Id, string PressName, string? LogoUrl, string Quote, string? SourceUrl, int SortOrder);

public record UpdateReviewRequest(string Title, string Content, string? FileUrl);
public record AdminReviewDto(int Id, string Title, string Content, string? FileUrl);

public record SettingItemDto(string Key, string Value, string? Description);
public record UpdateSettingsRequest(IReadOnlyList<SettingItemDto> Items);
