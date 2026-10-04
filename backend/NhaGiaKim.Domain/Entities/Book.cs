namespace NhaGiaKim.Domain.Entities;

/// <summary>Dau sach duoc ban tren landing page.</summary>
public class Book
{
    public int Id { get; set; }

    /// <summary>Ten sach, vi du "Nha Gia Kim".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The loai.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Title hien o Hero Section.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Subtitle hien o Hero Section.</summary>
    public string Subtitle { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    /// <summary>Gia sau giam. Null = khong giam gia.</summary>
    public decimal? DiscountPrice { get; set; }

    public string? CoverImageUrl { get; set; }

    /// <summary>Anh mockup dung o Hero Section.</summary>
    public string? MockupImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<BookImage> Images { get; set; } = new List<BookImage>();
    public Author? Author { get; set; }
    public ICollection<PressQuote> PressQuotes { get; set; } = new List<PressQuote>();
    public ContentReview? ContentReview { get; set; }
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
