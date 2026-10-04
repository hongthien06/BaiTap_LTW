namespace NhaGiaKim.Domain.Entities;

public class BookImage
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }

    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
}
