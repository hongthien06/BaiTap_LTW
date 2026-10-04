using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaGiaKim.Api.Security;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Common;

namespace NhaGiaKim.Api.Controllers.Admin;

[Route("api/admin/upload")]
[Authorize(Policy = AuthPolicies.RequireAdmin)]
public class AdminUploadController(IFileStorage storage) : ApiControllerBase
{
    /// <summary>
    /// Tai anh hoac file PDF len. Kiem tra bang magic bytes, khong tin duoi file.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(FileSignatureValidator.MaxPdfBytes)]
    [ProducesResponseType(typeof(StoredFile), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] UploadKind kind = UploadKind.Image,
        CancellationToken ct = default)
    {
        // Nem FileValidationException de middleware dung chung dung ra ProblemDetails co traceId,
        // thay vi tra ve mot hinh dang JSON thu ba chi rieng endpoint nay moi co.
        if (file is null || file.Length == 0)
            throw new FileValidationException("Chua chon file.");

        if (file.Length > FileSignatureValidator.MaxBytes(kind))
            throw new FileValidationException(
                $"File vuot qua gioi han {FileSignatureValidator.MaxBytes(kind) / 1024 / 1024} MB.");

        await using var stream = file.OpenReadStream();
        var stored = await storage.SaveAsync(stream, kind, ct);
        return StatusCode(StatusCodes.Status201Created, stored);
    }
}
