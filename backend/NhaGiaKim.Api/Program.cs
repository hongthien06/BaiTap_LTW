using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NhaGiaKim.Api.Middleware;
using NhaGiaKim.Api.Security;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Validators;
using NhaGiaKim.Domain.Enums;
using NhaGiaKim.Infrastructure;
using NhaGiaKim.Infrastructure.Security;
using NhaGiaKim.Infrastructure.Seed;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/nhagiakim-.log", rollingInterval: RollingInterval.Day));

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

// ---- JWT ----
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Thieu cau hinh Jwt.");
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key phai co it nhat 32 ky tu. Dat qua user-secrets (dev) hoac bien moi truong (prod).");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Chu ky hop le van chua du: tai khoan co the da bi khoa hoac bi ha quyen sau khi phat token.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ActiveUserValidator.ValidateAsync
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.RequireAdmin, p => p.RequireRole(RoleName.Admin))
    .AddPolicy(AuthPolicies.RequireStaffOrAdmin, p => p.RequireRole(RoleName.Admin, RoleName.Staff));

// ---- CORS: chi cho phep origin cua frontend ----
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];
builder.Services.AddCors(options => options.AddPolicy(CorsPolicies.Frontend, policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// ---- Rate limit cho endpoint public ghi du lieu (NFR-2) ----
var publicWritePermitLimit = builder.Configuration.GetValue<int?>("RateLimit:PublicWritePermitLimit") ?? 5;
var publicWriteWindowSeconds = builder.Configuration.GetValue<int?>("RateLimit:PublicWriteWindowSeconds") ?? 60;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(RateLimitPolicies.PublicWrite, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = publicWritePermitLimit,
                Window = TimeSpan.FromSeconds(publicWriteWindowSeconds),
                QueueLimit = 0
            }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nha Gia Kim API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhap token nhan duoc tu /api/auth/login"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// File tai len duoc phuc vu tu day. nosniff chan trinh duyet doan lai kieu noi dung
// (vector XSS voi file PDF polyglot), Content-Disposition buoc tai ve thay vi mo trong trang.
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        if (ctx.Context.Request.Path.StartsWithSegments("/uploads"))
        {
            ctx.Context.Response.Headers["Content-Disposition"] = "attachment";
        }
    }
});
app.UseCors(CorsPolicies.Frontend);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ---- Migrate + seed khi khoi dong (dev) ----
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, app.Configuration["Seed:AdminPassword"] ?? string.Empty);
}

app.Run();

/// <summary>Lo ra cho WebApplicationFactory trong test du an.</summary>
public partial class Program;
