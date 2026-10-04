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
        var user = await db.AppUsers.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return ServiceResult<AdminUserDto>.Fail(ServiceErrorCode.NotFound, "Khong tim thay tai khoan.");

        if (!ValidRoles.Contains(req.Role))
            return ServiceResult<AdminUserDto>.Invalid(nameof(req.Role), "Role khong hop le.");

        // Khong de he thong mat sach tai khoan Admin dang hoat dong.
        var adminRoleId = await db.Roles.Where(r => r.Name == RoleName.Admin).Select(r => r.Id).FirstAsync(ct);
        var isLastActiveAdmin = user.RoleId == adminRoleId && user.IsActive
            && !await db.AppUsers.AnyAsync(u => u.Id != id && u.RoleId == adminRoleId && u.IsActive, ct);

        if (isLastActiveAdmin && (req.Role != RoleName.Admin || !req.IsActive))
            return ServiceResult<AdminUserDto>.Invalid(nameof(req.Role), "Phai con it nhat mot tai khoan Admin dang hoat dong.");

        if (!string.IsNullOrEmpty(req.NewPassword))
        {
            if (req.NewPassword.Length < MinPasswordLength)
                return ServiceResult<AdminUserDto>.Invalid(nameof(req.NewPassword), $"Mat khau toi thieu {MinPasswordLength} ky tu.");
            user.PasswordHash = hasher.Hash(req.NewPassword);
        }

        var role = await db.Roles.FirstAsync(r => r.Name == req.Role, ct);
        user.FullName = req.FullName.Trim();
        user.RoleId = role.Id;
        user.Role = role;
        user.IsActive = req.IsActive;

        await db.SaveChangesAsync(ct);
        return ServiceResult<AdminUserDto>.Ok(ToDto(user));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, int currentUserId, CancellationToken ct = default)
    {
        if (id == currentUserId)
            return ServiceResult<bool>.Invalid("id", "Khong the tu xoa tai khoan dang dang nhap.");

        var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return ServiceResult<bool>.Fail(ServiceErrorCode.NotFound, "Khong tim thay tai khoan.");

        var adminRoleId = await db.Roles.Where(r => r.Name == RoleName.Admin).Select(r => r.Id).FirstAsync(ct);
        if (user.RoleId == adminRoleId
            && !await db.AppUsers.AnyAsync(u => u.Id != id && u.RoleId == adminRoleId && u.IsActive, ct))
        {
            return ServiceResult<bool>.Invalid("id", "Phai con it nhat mot tai khoan Admin dang hoat dong.");
        }

        db.AppUsers.Remove(user);
        await db.SaveChangesAsync(ct);
        return ServiceResult<bool>.Ok(true);
    }
}
