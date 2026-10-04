using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaGiaKim.Api.Security;
using NhaGiaKim.Application.Services;

namespace NhaGiaKim.Api.Controllers.Admin;

[Route("api/admin/feedbacks")]
[Authorize(Policy = AuthPolicies.RequireStaffOrAdmin)]
public class AdminFeedbacksController(IAdminFeedbackService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] bool? isApproved,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
        => ToResponse(await service.SearchAsync(isApproved, page, pageSize, ct));

    [HttpPatch("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
        => ToResponse(await service.SetApprovalAsync(id, true, ct));

    [HttpPatch("{id:int}/hide")]
    public async Task<IActionResult> Hide(int id, CancellationToken ct)
        => ToResponse(await service.SetApprovalAsync(id, false, ct));

    /// <summary>Xoa han: chi Admin.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthPolicies.RequireAdmin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => ToResponse(await service.DeleteAsync(id, ct));
}
