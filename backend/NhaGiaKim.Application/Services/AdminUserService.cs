using Microsoft.EntityFrameworkCore;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Domain.Entities;
using NhaGiaKim.Domain.Enums;

namespace NhaGiaKim.Application.Services;

public interface IAdminUserService
{
    Task<ServiceResult<IReadOnlyList<AdminUserDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<AdminUserDto>> CreateAsync(CreateUserRequest req, CancellationToken ct = default);
    Task<ServiceResult<AdminUserDto>> UpdateAsync(int id, UpdateUserRequest req, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteAsync(int id, int currentUserId, CancellationToken ct = default);
}

public class AdminUserService(IAppDbContext db, IPasswordHasher hasher, TimeProvider clock) : IAdminUserService
{
    public const int MinPasswordLength = 8;

    private static readonly string[] ValidRoles = [RoleName.Admin, RoleName.Staff];

    private static AdminUserDto ToDto(AppUser u) =>
        new(u.Id, u.Email, u.FullName, u.Role?.Name ?? string.Empty, u.IsActive, u.CreatedAt, u.LastLoginAt);

    public async Task<ServiceResult<IReadOnlyList<AdminUserDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await db.AppUsers.AsNoTracking()
            .Include(u => u.Role)
            .OrderBy(u => u.Id)
            .ToListAsync(ct);

        return ServiceResult<IReadOnlyList<AdminUserDto>>.Ok(users.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<AdminUserDto>> CreateAsync(CreateUserRequest req, CancellationToken ct = default)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        if (!ValidRoles.Contains(req.Role))
            return ServiceResult<AdminUserDto>.Invalid(nameof(req.Role), "Role khong hop le.");

        if (req.Password.Length < MinPasswordLength)
            return ServiceResult<AdminUserDto>.Invalid(nameof(req.Password), $"Mat khau toi thieu {MinPasswordLength} ky tu.");

        if (await db.AppUsers.AnyAsync(u => u.Email.ToLower() == email, ct))
            return ServiceResult<AdminUserDto>.Invalid(nameof(req.Email), "Email da duoc su dung.");

        var role = await db.Roles.FirstAsync(r => r.Name == req.Role, ct);
        var user = new AppUser
        {
            Email = email,
            PasswordHash = hasher.Hash(req.Password),
            FullName = req.FullName.Trim(),
            RoleId = role.Id,
            Role = role,
            IsActive = true,
            CreatedAt = clock.GetUtcNow().UtcDateTime
        };

        db.AppUsers.Add(user);
        await db.SaveChangesAsync(ct);

        return ServiceResult<AdminUserDto>.Ok(ToDto(user));
    }

    public async Task<ServiceResult<AdminUserDto>> UpdateAsync(int id, UpdateUserRequest req, CancellationToken ct = default)
    {
        if (!ValidRoles.Contains(req.Role))
            return ServiceResult<AdminUserDto>.Invalid(nameof(req.Role), "Role khong hop le.");

        if (!string.IsNullOrEmpty(req.NewPassword) && req.NewPassword.Length < MinPasswordLength)
            return ServiceResult<AdminUserDto>.Invalid(nameof(req.NewPassword), $"Mat khau toi thieu {MinPasswordLength} ky tu.");

        // Doc-roi-ghi tren mot bat bien toan cuc: hai request song song cung ha quyen hai Admin
        // cuoi cung se deu thay "van con Admin khac" neu khong tuan tu hoa.
        return await db.ExecuteInSerializableTransactionAsync<ServiceResult<AdminUserDto>>(async token =>
        {
            var user = await db.AppUsers.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id, token);
            if (user is null)
            {
                return (false, ServiceResult<AdminUserDto>.Fail(
                    ServiceErrorCode.NotFound, "Khong tim thay tai khoan."));
            }

            if (!string.IsNullOrEmpty(req.NewPassword))
                user.PasswordHash = hasher.Hash(req.NewPassword);

            var role = await db.Roles.FirstAsync(r => r.Name == req.Role, token);
            user.FullName = req.FullName.Trim();
            user.RoleId = role.Id;
            user.Role = role;
            user.IsActive = req.IsActive;

            await db.SaveChangesAsync(token);

            // Kiem tra SAU khi ghi, trong cung transaction: neu thao tac nay lam he thong het
            // Admin dang hoat dong thi rollback. Dung cho moi duong (ha quyen, khoa, xoa).
            if (!await HasActiveAdminAsync(token))
            {
                return (false, ServiceResult<AdminUserDto>.Invalid(nameof(req.Role), LastAdminMessage));
            }

            return (true, ServiceResult<AdminUserDto>.Ok(ToDto(user)));
        }, ct);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, int currentUserId, CancellationToken ct = default)
    {
        if (id == currentUserId)
            return ServiceResult<bool>.Invalid("id", "Khong the tu xoa tai khoan dang dang nhap.");

        return await db.ExecuteInSerializableTransactionAsync<ServiceResult<bool>>(async token =>
        {
            var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == id, token);
            if (user is null)
            {
                return (false, ServiceResult<bool>.Fail(ServiceErrorCode.NotFound, "Khong tim thay tai khoan."));
            }

            db.AppUsers.Remove(user);
            await db.SaveChangesAsync(token);

            if (!await HasActiveAdminAsync(token))
            {
                return (false, ServiceResult<bool>.Invalid("id", LastAdminMessage));
            }

            return (true, ServiceResult<bool>.Ok(true));
        }, ct);
    }

    private const string LastAdminMessage = "Phai con it nhat mot tai khoan Admin dang hoat dong.";

    /// <summary>Con it nhat mot tai khoan Admin dang hoat dong khong?</summary>
    private Task<bool> HasActiveAdminAsync(CancellationToken ct) =>
        db.AppUsers.AnyAsync(u => u.IsActive && u.Role!.Name == RoleName.Admin, ct);
}
