namespace NhaGiaKim.Application.Dtos.Admin;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, CurrentUserDto User);

public record CurrentUserDto(int Id, string Email, string FullName, string Role);
