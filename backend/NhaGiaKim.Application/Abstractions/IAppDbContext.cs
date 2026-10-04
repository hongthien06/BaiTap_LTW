using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

    /// <summary>Truy cap change tracker, dung de go mot entity ra khi ghi that bai.</summary>
    EntityEntry Entry(object entity);

    /// <summary>
    /// Chay mot khoi lenh trong transaction SERIALIZABLE, commit khi operation tra Commit = true.
    ///
    /// Can cho cac luat kieu "phai con it nhat mot Admin dang hoat dong": doc roi ghi ma khong
    /// tuan tu hoa thi hai request song song deu thay dieu kien con dung.
    ///
    /// Phai di qua ham nay chu khong tu goi BeginTransactionAsync: provider SQL Server dang bat
    /// EnableRetryOnFailure, va execution strategy do tu choi transaction do nguoi dung tu mo
    /// ("does not support user-initiated transactions").
    /// </summary>
    Task<T> ExecuteInSerializableTransactionAsync<T>(
        Func<CancellationToken, Task<(bool Commit, T Result)>> operation, CancellationToken ct = default);
}
