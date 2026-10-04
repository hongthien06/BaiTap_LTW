using System;

namespace NhaGiaKim.Domain.Entities;

/// <summary>Tai khoan quan tri.</summary>
public class AppUser
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;

    /// <summary>Hash BCrypt. Khong bao gio luu plaintext.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
