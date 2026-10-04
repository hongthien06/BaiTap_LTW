using Microsoft.AspNetCore.Mvc;
using NhaGiaKim.Application.Abstractions;

namespace NhaGiaKim.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Doi ServiceResult thanh HTTP response thong nhat.</summary>
    protected IActionResult ToResponse<T>(ServiceResult<T> result, int successStatus = StatusCodes.Status200OK)
    {
        if (result.Succeeded)
        {
            return successStatus == StatusCodes.Status201Created
                ? StatusCode(StatusCodes.Status201Created, result.Value)
                : StatusCode(successStatus, result.Value);
        }

        var status = result.ErrorCode switch
        {
            ServiceErrorCode.Validation => StatusCodes.Status400BadRequest,
            ServiceErrorCode.NotFound => StatusCodes.Status404NotFound,
            ServiceErrorCode.Conflict => StatusCodes.Status409Conflict,
            ServiceErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
            ServiceErrorCode.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        var problem = new ValidationProblemDetails(
            result.Errors.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
            Type = result.ErrorCode.ToString().ToLowerInvariant(),
            Title = result.ErrorMessage,
            Status = status,
            Detail = result.ErrorMessage,
            Extensions = { ["traceId"] = HttpContext.TraceIdentifier }
        };

        return StatusCode(status, problem);
    }
}
