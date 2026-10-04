using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace NhaGiaKim.Api.Middleware;

/// <summary>
/// Mot cho duy nhat bien exception thanh ProblemDetails kem traceId.
/// Khong bao gio tra chi tiet exception ra ngoai o moi truong production.
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, "bad_request", ex.Message);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            logger.LogError(ex, "Loi chua xu ly. TraceId={TraceId}", traceId);

            var detail = env.IsDevelopment() ? ex.Message : "Da co loi xay ra, vui long thu lai.";
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "server_error", detail);
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string type, string detail)
    {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = type,
            Title = status == StatusCodes.Status400BadRequest ? "Yeu cau khong hop le" : "Loi he thong",
            Status = status,
            Detail = detail,
            Extensions = { ["traceId"] = context.TraceIdentifier }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
