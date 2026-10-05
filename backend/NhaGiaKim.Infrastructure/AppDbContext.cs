using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

    /// <summary>
    /// Danh dau lai moi DateTime doc tu DB la UTC.
    ///
    /// Vi sao can: cot datetime2 cua SQL Server KHONG luu mui gio, nen EF Core tra ve
    /// DateTime voi Kind = Unspecified. System.Text.Json thay Unspecified thi ghi ra chuoi
    /// KHONG co hau to "Z". Trinh duyet gap chuoi khong co Z thi hieu la GIO DIA PHUONG.
    ///
    /// Hau qua that da gap: don vua dat luc 18:27 gio Viet Nam (11:27 UTC) duoc tra ve la
    /// "2026-10-05T11:27:52" -> trinh duyet hieu thanh 11:27 gio Viet Nam, tuc 7 tieng truoc
    /// -> vuot nguong 4 tieng -> bi gan nhan "Qua han xu ly" ngay khi vua dat xong.
    /// Gio hien thi tren man hinh cung lech 7 tieng.
    ///
    /// Moi duong ghi deu dung clock.GetUtcNow() nen gia tri trong DB von da la UTC;
    /// o day chi noi lai cho .NET biet dieu do.
    /// </summary>
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
        v => v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> NullableUtcConverter = new(
        v => v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(UtcConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(NullableUtcConverter);
                }
            }
        }
    }
}
