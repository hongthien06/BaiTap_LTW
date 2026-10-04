namespace NhaGiaKim.Domain.Entities;

/// <summary>Review noi dung sach, kem file review tai ve duoc.</summary>
public class ContentReview
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    /// <summary>Duong dan file review (PDF). Null = khong co file de tai.</summary>
    public string? FileUrl { get; set; }
}
