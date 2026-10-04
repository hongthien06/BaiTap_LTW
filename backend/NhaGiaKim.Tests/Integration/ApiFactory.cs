using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NhaGiaKim.Infrastructure;
using NhaGiaKim.Infrastructure.Seed;

namespace NhaGiaKim.Tests.Integration;

/// <summary>
/// Dung API that voi SQLite in-memory thay cho SQL Server.
///
/// Cau hinh phai di qua BIEN MOI TRUONG chu khong phai ConfigureAppConfiguration:
/// Program.cs doc builder.Configuration ngay khi dung host (de validate Jwt:Key va rate limit),
/// thoi diem do cac nguon config do WebApplicationFactory them vao CHUA duoc ap dung.
/// Neu dung ConfigureAppConfiguration, token se duoc ky bang key test nhung lai duoc
/// kiem tra bang key trong user-secrets -> moi request kem token deu 401.
///
/// Vi bien moi truong la pham vi tien trinh, test chay tuan tu (xem TestCollectionBehavior.cs).
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = DbSeeder.DefaultAdminEmail;
    public const string AdminPassword = "Admin@12345";

    private const string TestJwtKey = "test-only-signing-key-at-least-32-characters-long-0123456789";

    private DbConnection? _connection;

    /// <summary>Gioi han rate limit cho factory nay. Mac dinh noi rong de khong lam nhieu test khac.</summary>
    protected virtual int PublicWritePermitLimit => 1000;

    public ApiFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", TestJwtKey);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "NhaGiaKim.Api");
        Environment.SetEnvironmentVariable("Jwt__Audience", "NhaGiaKim.Client");
        Environment.SetEnvironmentVariable("Jwt__ExpiryMinutes", "60");
        Environment.SetEnvironmentVariable("Database__MigrateOnStartup", "false");
        Environment.SetEnvironmentVariable("Seed__AdminPassword", AdminPassword);
        Environment.SetEnvironmentVariable("RateLimit__PublicWritePermitLimit", PublicWritePermitLimit.ToString());
        Environment.SetEnvironmentVariable("RateLimit__PublicWriteWindowSeconds", "60");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureServices(services =>
        {
            // Bo dang ky SQL Server, thay bang SQLite in-memory giu mo suot vong doi factory.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(db, AdminPassword);
    }

    public new async Task DisposeAsync()
    {
        if (_connection is not null) await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}

/// <summary>Factory rieng cho test rate limit: that chat gioi han xuong 5 request/phut.</summary>
public class RateLimitedApiFactory : ApiFactory
{
    protected override int PublicWritePermitLimit => 5;
}
