using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NhaGiaKim.Application.Abstractions;

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
    private const string CorsHeaderPrefix = "Access-Control-";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (FileValidationException ex)
        {
            // Loi cua nguoi goi, khong phai su co he thong -> log muc warning, tra 400 kem ly do.
            logger.LogWarning(ex, "File tai len khong hop le. TraceId={TraceId}", context.TraceIdentifier);
            await WriteAsync(context, StatusCodes.Status400BadRequest, "validation_error",
                "Yeu cau khong hop le", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Loi chua xu ly. TraceId={TraceId}", context.TraceIdentifier);

            var detail = env.IsDevelopment() ? ex.Message : "Da co loi xay ra, vui long thu lai.";
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "server_error",
                "Loi he thong", detail);
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string type, string title, string detail)
    {
        if (context.Response.HasStarted) return;

        // Response.Clear() xoa ca header CORS do middleware CORS da ghi. Mat header do thi
        // trinh duyet chan response loi nhu loi CORS, FE khong doc duoc body - tuc khong
        // bao gio lay duoc traceId ma NFR-8 yeu cau. Giu lai roi dat lai sau khi clear.
        var corsHeaders = context.Response.Headers
            .Where(h => h.Key.StartsWith(CorsHeaderPrefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        context.Response.Clear();

        foreach (var header in corsHeaders)
        {
            context.Response.Headers[header.Key] = header.Value;
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = type,
            Title = title,
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
