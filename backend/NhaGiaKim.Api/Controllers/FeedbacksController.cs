using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NhaGiaKim.Api.Security;
using NhaGiaKim.Application.Dtos.Public;
using NhaGiaKim.Application.Services;

namespace NhaGiaKim.Api.Controllers;

[Route("api/feedbacks")]
public class FeedbacksController(IFeedbackService feedbackService) : ApiControllerBase
{
    /// <summary>Gui danh gia. Luon o trang thai cho duyet.</summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.PublicWrite)]
    [ProducesResponseType(typeof(CreateFeedbackResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackRequest request, CancellationToken ct)
        => ToResponse(await feedbackService.CreateAsync(request, ct), StatusCodes.Status201Created);
}
