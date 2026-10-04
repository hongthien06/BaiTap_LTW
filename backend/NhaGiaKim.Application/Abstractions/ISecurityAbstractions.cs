using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Domain.Entities;

namespace NhaGiaKim.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);

    /// <summary>
    /// Hash cua mot chuoi ngau nhien, dung de verify khi email khong ton tai.
    /// Khong co no thi email sai se tra ve sau ~2ms con email dung sai mat khau mat ~300ms
    /// (BCrypt work factor 12) - chenh lech do du de do xem email nao co that.
    /// </summary>
    string DummyHash { get; }
}

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) Generate(AppUser user, string roleName);
}

/// <summary>Thong tin user dang dang nhap, lay tu JWT claims.</summary>
public interface ICurrentUser
{
    int? UserId { get; }
    string? Email { get; }
    string? Role { get; }
}
