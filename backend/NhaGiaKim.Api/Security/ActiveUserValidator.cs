using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using NhaGiaKim.Application.Abstractions;

namespace NhaGiaKim.Api.Security;

/// <summary>
/// Doi chieu JWT voi trang thai THAT trong DB o moi request da xac thuc.
///
/// JWT la stateless: sau khi phat, noi dung trong token khong doi cho toi khi het han (60 phut).
/// Neu khong kiem tra lai, admin khoa mot tai khoan hoac ha quyen Admin xuong Staff se KHONG
/// co tac dung ngay - token cu van vao duoc panel admin trong toi da 60 phut.
///
/// Danh doi: them mot truy van DB nho cho moi request co token. Chap nhan duoc o quy mo bai tap,
/// va la danh doi dung cho khu vuc quan tri. Neu luu luong lon thi nen cache theo userId vai giay.
/// </summary>
public static class ActiveUserValidator
{
    public static async Task ValidateAsync(TokenValidatedContext context)
    {
        var principal = context.Principal;
        if (principal is null)
        {
            context.Fail("Token khong hop le.");
            return;
        }

        if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            context.Fail("Token thieu dinh danh nguoi dung.");
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<IAppDbContext>();

        var current = await db.AppUsers
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.IsActive, RoleName = u.Role!.Name })
            .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

        if (current is null || !current.IsActive)
        {
            // Tai khoan da bi xoa hoac bi khoa -> thu hoi phien ngay.
            context.Fail("Tai khoan khong con hieu luc.");
            return;
        }

        // Role trong token phai khop role hien tai; neu admin vua ha quyen thi buoc dang nhap lai.
        var tokenRole = principal.FindFirstValue(ClaimTypes.Role);
        if (!string.Equals(tokenRole, current.RoleName, StringComparison.Ordinal))
        {
            context.Fail("Quyen cua tai khoan da thay doi, vui long dang nhap lai.");
        }
    }
}
