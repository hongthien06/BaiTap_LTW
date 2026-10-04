using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Domain.Entities;

namespace NhaGiaKim.Infrastructure;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookImage> BookImages => Set<BookImage>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<PressQuote> PressQuotes => Set<PressQuote>();
    public DbSet<ContentReview> ContentReviews => Set<ContentReview>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    /// <summary>
    /// Serializable de chan "phantom read": luat nhu "phai con it nhat mot Admin dang hoat dong"
    /// doc mot tap hop roi ghi, muc co lap thap hon khong chan duoc hai request song song
    /// cung thay dieu kien van con dung.
    ///
    /// Bat buoc chay qua CreateExecutionStrategy: voi EnableRetryOnFailure, EF Core tu choi
    /// transaction tu mo bang BeginTransactionAsync. SQLite (dung trong test) khong bat retry nen
    /// loi do chi xuat hien tren SQL Server - mot sai lech moi truong de lot luoi.
    /// </summary>
    public Task<T> ExecuteInSerializableTransactionAsync<T>(
        Func<CancellationToken, Task<(bool Commit, T Result)>> operation, CancellationToken ct = default)
    {
        var strategy = Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async token =>
        {
            await using var transaction = await Database.BeginTransactionAsync(IsolationLevel.Serializable, token);

            var (commit, result) = await operation(token);

            if (commit) await transaction.CommitAsync(token);
            else await transaction.RollbackAsync(token);

            return result;
        }, ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
