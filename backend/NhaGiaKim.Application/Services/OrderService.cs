using Microsoft.EntityFrameworkCore;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Common;
using NhaGiaKim.Application.Dtos.Public;
using NhaGiaKim.Domain.Entities;
using NhaGiaKim.Domain.Enums;

namespace NhaGiaKim.Application.Services;

public interface IOrderService
{
    Task<ServiceResult<CreateOrderResponse>> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
}

public class OrderService(IAppDbContext db, TimeProvider clock) : IOrderService
{
    private const int MaxCodeAttempts = 3;

    public async Task<ServiceResult<CreateOrderResponse>> CreateAsync(
        CreateOrderRequest request, CancellationToken ct = default)
    {
        var book = await db.Books.AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Id)
            .FirstOrDefaultAsync(ct);

        if (book is null)
        {
            return ServiceResult<CreateOrderResponse>.Fail(
                ServiceErrorCode.NotFound, "Khong tim thay sach dang mo ban.");
        }

        // AC-10: gia LUON tinh lai tu DB, bo qua moi gia tri gia do client gui len.
        var unitPrice = MoneyCalculator.EffectiveUnitPrice(book.Price, book.DiscountPrice);
        var total = MoneyCalculator.Total(unitPrice, request.Quantity);
        var now = clock.GetUtcNow().UtcDateTime;

        for (var attempt = 1; attempt <= MaxCodeAttempts; attempt++)
        {
            var order = new Order
            {
                OrderCode = OrderCodeGenerator.Generate(now),
                BookId = book.Id,
                CustomerName = request.CustomerName.Trim(),
                Phone = request.Phone.Trim(),
                Address = request.Address.Trim(),
                Quantity = request.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = total,
                PaymentMethod = request.PaymentMethod,
                Status = OrderStatus.New,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            db.Orders.Add(order);
            try
            {
                await db.SaveChangesAsync(ct);
                return ServiceResult<CreateOrderResponse>.Ok(
                    new CreateOrderResponse(order.OrderCode, unitPrice, total));
            }
            catch (DbUpdateException) when (attempt < MaxCodeAttempts)
            {
                // Unique index tren OrderCode da chan trung; sinh ma moi roi thu lai.
                db.Orders.Remove(order);
            }
        }

        return ServiceResult<CreateOrderResponse>.Fail(
            ServiceErrorCode.Conflict, "Khong sinh duoc ma don, vui long thu lai.");
    }
}
