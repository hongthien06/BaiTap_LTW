using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Domain.Entities;

namespace NhaGiaKim.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
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
