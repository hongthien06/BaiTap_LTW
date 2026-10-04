using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NhaGiaKim.Api.Security;
using NhaGiaKim.Application.Dtos.Public;
using NhaGiaKim.Application.Services;

namespace NhaGiaKim.Api.Controllers;

[Route("api/orders")]
public class OrdersController(IOrderService orderService) : ApiControllerBase
{
    /// <summary>Tao don dat hang (guest checkout). Gia luon do server tinh lai.</summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.PublicWrite)]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        => ToResponse(await orderService.CreateAsync(request, ct), StatusCodes.Status201Created);
}
