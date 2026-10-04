using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaGiaKim.Api.Security;
using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Application.Services;
using NhaGiaKim.Domain.Enums;

namespace NhaGiaKim.Api.Controllers.Admin;

/// <summary>Don hang: ca Admin va Staff deu xem va doi trang thai duoc.</summary>
[Route("api/admin/orders")]
[Authorize(Policy = AuthPolicies.RequireStaffOrAdmin)]
public class AdminOrdersController(IAdminOrderService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] OrderStatus? status,
        [FromQuery] string? phone,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
        => ToResponse(await service.SearchAsync(new OrderListQuery(status, phone, from, to, page, pageSize), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => ToResponse(await service.GetByIdAsync(id, ct));

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest req, CancellationToken ct)
        => ToResponse(await service.UpdateStatusAsync(id, req, ct));
}
