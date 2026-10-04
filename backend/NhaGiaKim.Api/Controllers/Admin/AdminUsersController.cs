using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaGiaKim.Api.Security;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Application.Services;

namespace NhaGiaKim.Api.Controllers.Admin;

[Route("api/admin/users")]
[Authorize(Policy = AuthPolicies.RequireAdmin)]
public class AdminUsersController(IAdminUserService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => ToResponse(await service.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
        => ToResponse(await service.CreateAsync(req, ct), StatusCodes.Status201Created);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req, CancellationToken ct)
        => ToResponse(await service.UpdateAsync(id, req, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var me = currentUser.UserId;
        if (me is null) return Unauthorized();
        return ToResponse(await service.DeleteAsync(id, me.Value, ct));
    }
}
