namespace NhaGiaKim.Domain.Entities;

/// <summary>Thong tin tac gia, hien o section "Chi tiet tac gia".</summary>
public class Author
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    /// <summary>Tieu su tac gia.</summary>
    public string Bio { get; set; } = string.Empty;
}
