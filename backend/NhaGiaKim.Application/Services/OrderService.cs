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

public class OrderService(IAppDbContext db, TimeProvider clock, IDbExceptionClassifier dbErrors) : IOrderService
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
            catch (DbUpdateException ex) when (dbErrors.IsUniqueConstraintViolation(ex))
            {
                // Chi retry khi dung la trung ma don (unique index da chan).
                // Moi loi ghi khac - timeout, mat ket noi, vi pham khoa ngoai - phai noi len
                // thanh 500 chu khong duoc nuot roi ghi lai them hai lan nua.
                //
                // Detach chu khong Remove: entity dang o trang thai Added, muc dich la go no
                // khoi change tracker, khong phai xoa mot ban ghi da ton tai.
                db.Entry(order).State = EntityState.Detached;
            }
        }

        // Het so lan thu ma van trung -> bao 409 de client thu lai, khong de exception thanh 500.
        return ServiceResult<CreateOrderResponse>.Fail(
            ServiceErrorCode.Conflict, "Khong sinh duoc ma don, vui long thu lai.");
    }
}
