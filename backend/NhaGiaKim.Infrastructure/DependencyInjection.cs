using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Services;
using NhaGiaKim.Infrastructure.Security;
using NhaGiaKim.Infrastructure.Storage;

namespace NhaGiaKim.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config, string contentRootPath)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Thieu ConnectionStrings:Default.");

        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString, sql =>
            sql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        var storageOptions = new LocalFileStorageOptions();
        config.GetSection(LocalFileStorageOptions.SectionName).Bind(storageOptions);
        if (string.IsNullOrWhiteSpace(storageOptions.RootPath))
            storageOptions.RootPath = Path.Combine(contentRootPath, "wwwroot", "uploads");
        services.AddSingleton(storageOptions);
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        services.AddScoped<ILandingService, LandingService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminContentService, AdminContentService>();
        services.AddScoped<IAdminOrderService, AdminOrderService>();
        services.AddScoped<IAdminFeedbackService, AdminFeedbackService>();
        services.AddScoped<IAdminUserService, AdminUserService>();

        return services;
    }
}
