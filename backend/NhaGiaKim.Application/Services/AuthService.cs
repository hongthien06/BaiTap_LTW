using Microsoft.EntityFrameworkCore;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Dtos.Admin;

namespace NhaGiaKim.Application.Services;

public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<ServiceResult<CurrentUserDto>> GetMeAsync(int userId, CancellationToken ct = default);
}

public class AuthService(
    IAppDbContext db,
    IPasswordHasher hasher,
    IJwtTokenGenerator jwt,
    TimeProvider clock) : IAuthService
{
    /// <summary>
    /// AC-18: sai email hay sai mat khau deu tra ve CUNG MOT thong bao,
    /// khong tiet lo email co ton tai hay khong.
    /// </summary>
    private const string InvalidCredentials = "Email hoac mat khau khong dung.";

    public async Task<ServiceResult<LoginResponse>> LoginAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await db.AppUsers
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email, ct);

        if (user is null || !user.IsActive || !hasher.Verify(request.Password, user.PasswordHash))
        {
            return ServiceResult<LoginResponse>.Fail(ServiceErrorCode.Unauthorized, InvalidCredentials);
        }

        var roleName = user.Role?.Name ?? string.Empty;
        var (token, expiresAt) = jwt.Generate(user, roleName);

        user.LastLoginAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);

        return ServiceResult<LoginResponse>.Ok(new LoginResponse(
            token, expiresAt,
            new CurrentUserDto(user.Id, user.Email, user.FullName, roleName)));
    }

    public async Task<ServiceResult<CurrentUserDto>> GetMeAsync(int userId, CancellationToken ct = default)
    {
        var user = await db.AppUsers.AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);

        return user is null
            ? ServiceResult<CurrentUserDto>.Fail(ServiceErrorCode.Unauthorized, "Phien dang nhap khong hop le.")
            : ServiceResult<CurrentUserDto>.Ok(new CurrentUserDto(
                user.Id, user.Email, user.FullName, user.Role?.Name ?? string.Empty));
    }
}
