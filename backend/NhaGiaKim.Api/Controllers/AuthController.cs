using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Application.Services;

namespace NhaGiaKim.Api.Controllers;

[Route("api/auth")]
public class AuthController(IAuthService authService, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        => ToResponse(await authService.LoginAsync(request, ct));

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (userId is null) return Unauthorized();
        return ToResponse(await authService.GetMeAsync(userId.Value, ct));
    }
}
