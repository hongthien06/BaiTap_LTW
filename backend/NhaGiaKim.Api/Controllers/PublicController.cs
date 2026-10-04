using Microsoft.AspNetCore.Mvc;
using NhaGiaKim.Application.Dtos.Public;
using NhaGiaKim.Application.Services;

namespace NhaGiaKim.Api.Controllers;

[Route("api/public")]
public class PublicController(ILandingService landingService) : ApiControllerBase
{
    /// <summary>Tra toan bo du lieu landing page trong MOT request.</summary>
    [HttpGet("landing")]
    [ProducesResponseType(typeof(LandingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLanding(CancellationToken ct)
        => ToResponse(await landingService.GetAsync(ct));
}
