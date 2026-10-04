using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaGiaKim.Api.Security;
using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Application.Services;

namespace NhaGiaKim.Api.Controllers.Admin;

/// <summary>Quan ly noi dung hien tren landing. Chi Admin duoc sua (AC-20).</summary>
[Route("api/admin")]
[Authorize(Policy = AuthPolicies.RequireAdmin)]
public class AdminContentController(IAdminContentService service) : ApiControllerBase
{
    [HttpGet("book")]
    public async Task<IActionResult> GetBook(CancellationToken ct) => ToResponse(await service.GetBookAsync(ct));

    [HttpPut("book")]
    public async Task<IActionResult> UpdateBook([FromBody] UpdateBookRequest req, CancellationToken ct)
        => ToResponse(await service.UpdateBookAsync(req, ct));

    [HttpGet("author")]
    public async Task<IActionResult> GetAuthor(CancellationToken ct) => ToResponse(await service.GetAuthorAsync(ct));

    [HttpPut("author")]
    public async Task<IActionResult> UpsertAuthor([FromBody] UpdateAuthorRequest req, CancellationToken ct)
        => ToResponse(await service.UpsertAuthorAsync(req, ct));

    [HttpGet("press-quotes")]
    public async Task<IActionResult> GetPressQuotes(CancellationToken ct)
        => ToResponse(await service.GetPressQuotesAsync(ct));

    [HttpPost("press-quotes")]
    public async Task<IActionResult> CreatePressQuote([FromBody] PressQuoteRequest req, CancellationToken ct)
        => ToResponse(await service.CreatePressQuoteAsync(req, ct), StatusCodes.Status201Created);

    [HttpPut("press-quotes/{id:int}")]
    public async Task<IActionResult> UpdatePressQuote(int id, [FromBody] PressQuoteRequest req, CancellationToken ct)
        => ToResponse(await service.UpdatePressQuoteAsync(id, req, ct));

    [HttpDelete("press-quotes/{id:int}")]
    public async Task<IActionResult> DeletePressQuote(int id, CancellationToken ct)
        => ToResponse(await service.DeletePressQuoteAsync(id, ct));

    [HttpGet("review")]
    public async Task<IActionResult> GetReview(CancellationToken ct) => ToResponse(await service.GetReviewAsync(ct));

    [HttpPut("review")]
    public async Task<IActionResult> UpsertReview([FromBody] UpdateReviewRequest req, CancellationToken ct)
        => ToResponse(await service.UpsertReviewAsync(req, ct));

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct) => ToResponse(await service.GetSettingsAsync(ct));

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest req, CancellationToken ct)
        => ToResponse(await service.UpdateSettingsAsync(req, ct));
}
