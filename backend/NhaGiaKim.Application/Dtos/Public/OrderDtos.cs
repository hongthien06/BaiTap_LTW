using NhaGiaKim.Domain.Enums;

namespace NhaGiaKim.Application.Dtos.Public;

/// <summary>
/// Payload tao don. KHONG co truong gia: server luon tu tinh lai tu DB (AC-10).
/// </summary>
public record CreateOrderRequest(
    string CustomerName,
    string Phone,
    string Address,
    int Quantity,
    PaymentMethod PaymentMethod,
    string? Note);

public record CreateOrderResponse(string OrderCode, decimal UnitPrice, decimal TotalPrice);

public record CreateFeedbackRequest(string CustomerName, int Rating, string Content);

public record CreateFeedbackResponse(int Id, string Message);
