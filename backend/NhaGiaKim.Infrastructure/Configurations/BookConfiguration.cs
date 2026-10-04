using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NhaGiaKim.Domain.Entities;

namespace NhaGiaKim.Infrastructure.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> b)
    {
        b.ToTable("Books");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Category).HasMaxLength(100).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Subtitle).HasMaxLength(500);
        b.Property(x => x.Price).HasPrecision(18, 2);
        b.Property(x => x.DiscountPrice).HasPrecision(18, 2);
        b.Property(x => x.CoverImageUrl).HasMaxLength(500);
        b.Property(x => x.MockupImageUrl).HasMaxLength(500);
        b.HasIndex(x => x.IsActive);

        b.HasMany(x => x.Images).WithOne(x => x.Book!).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Author).WithOne(x => x.Book!).HasForeignKey<Author>(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.PressQuotes).WithOne(x => x.Book!).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ContentReview).WithOne(x => x.Book!).HasForeignKey<ContentReview>(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Feedbacks).WithOne(x => x.Book!).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        // Don hang KHONG bi xoa theo sach - giu lich su.
        b.HasMany(x => x.Orders).WithOne(x => x.Book!).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BookImageConfiguration : IEntityTypeConfiguration<BookImage>
{
    public void Configure(EntityTypeBuilder<BookImage> b)
    {
        b.ToTable("BookImages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Url).HasMaxLength(500).IsRequired();
        b.Property(x => x.Caption).HasMaxLength(300);
    }
}

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> b)
    {
        b.ToTable("Authors");
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.AvatarUrl).HasMaxLength(500);
    }
}

public class PressQuoteConfiguration : IEntityTypeConfiguration<PressQuote>
{
    public void Configure(EntityTypeBuilder<PressQuote> b)
    {
        b.ToTable("PressQuotes");
        b.HasKey(x => x.Id);
        b.Property(x => x.PressName).HasMaxLength(200).IsRequired();
        b.Property(x => x.LogoUrl).HasMaxLength(500);
        b.Property(x => x.Quote).HasMaxLength(1000).IsRequired();
        b.Property(x => x.SourceUrl).HasMaxLength(500);
    }
}

public class ContentReviewConfiguration : IEntityTypeConfiguration<ContentReview>
{
    public void Configure(EntityTypeBuilder<ContentReview> b)
    {
        b.ToTable("ContentReviews");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.FileUrl).HasMaxLength(500);
    }
}
