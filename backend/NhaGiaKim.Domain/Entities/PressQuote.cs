namespace NhaGiaKim.Domain.Entities;

/// <summary>Trich dan bao chi viet ve sach.</summary>
public class PressQuote
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }

    public string PressName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string Quote { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public int SortOrder { get; set; }
}
