using Microsoft.EntityFrameworkCore;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
