namespace NhaGiaKim.Application.Dtos.Admin;

public record CreateUserRequest(string Email, string Password, string FullName, string Role);
public record UpdateUserRequest(string FullName, string Role, bool IsActive, string? NewPassword);
public record AdminUserDto(int Id, string Email, string FullName, string Role, bool IsActive, DateTime CreatedAt, DateTime? LastLoginAt);
