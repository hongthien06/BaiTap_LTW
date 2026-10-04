using Microsoft.EntityFrameworkCore;
using NhaGiaKim.Domain.Entities;

namespace NhaGiaKim.Application.Abstractions;

/// <summary>
/// Cua ngo du lieu cho tang Application. EF Core DbContext da dong vai tro Unit of Work +
/// Repository nen khong them lop repository mong o giua.
/// </summary>
public interface IAppDbContext
{
    DbSet<Book> Books { get; }
    DbSet<BookImage> BookImages { get; }
    DbSet<Author> Authors { get; }
    DbSet<PressQuote> PressQuotes { get; }
    DbSet<ContentReview> ContentReviews { get; }
    DbSet<Feedback> Feedbacks { get; }
    DbSet<Order> Orders { get; }
    DbSet<AppUser> AppUsers { get; }
    DbSet<Role> Roles { get; }
    DbSet<SiteSetting> SiteSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
