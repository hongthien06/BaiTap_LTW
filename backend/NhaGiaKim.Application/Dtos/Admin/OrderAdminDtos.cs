using NhaGiaKim.Domain.Enums;

namespace NhaGiaKim.Application.Dtos.Admin;

public record OrderListQuery(
    OrderStatus? Status = null,
    string? Phone = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 10);

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public record AdminOrderDto(
    int Id, string OrderCode, string CustomerName, string Phone, string Address,
    int Quantity, decimal UnitPrice, decimal TotalPrice,
    PaymentMethod PaymentMethod, OrderStatus Status, string? Note,
    DateTime CreatedAt, DateTime UpdatedAt);

public record UpdateOrderStatusRequest(OrderStatus Status);

public record AdminFeedbackDto(
    int Id, string CustomerName, int Rating, string Content,
    bool IsApproved, DateTime CreatedAt, DateTime? ApprovedAt);
