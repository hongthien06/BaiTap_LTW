using Microsoft.EntityFrameworkCore;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Common;
using NhaGiaKim.Application.Dtos.Admin;

namespace NhaGiaKim.Application.Services;

public interface IAdminOrderService
{
    Task<ServiceResult<PagedResult<AdminOrderDto>>> SearchAsync(OrderListQuery query, CancellationToken ct = default);
    Task<ServiceResult<AdminOrderDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<AdminOrderDto>> UpdateStatusAsync(int id, UpdateOrderStatusRequest req, CancellationToken ct = default);
}

public class AdminOrderService(IAppDbContext db, TimeProvider clock) : IAdminOrderService
{
    public const int MaxPageSize = 100;

    private static AdminOrderDto ToDto(Domain.Entities.Order o) => new(
        o.Id, o.OrderCode, o.CustomerName, o.Phone, o.Address, o.Quantity,
        o.UnitPrice, o.TotalPrice, o.PaymentMethod, o.Status, o.Note, o.CreatedAt, o.UpdatedAt);

    public async Task<ServiceResult<PagedResult<AdminOrderDto>>> SearchAsync(
        OrderListQuery query, CancellationToken ct = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > MaxPageSize ? 10 : query.PageSize;

        var q = db.Orders.AsNoTracking().AsQueryable();

        if (query.Status.HasValue)
            q = q.Where(o => o.Status == query.Status.Value);

        if (!string.IsNullOrWhiteSpace(query.Phone))
        {
            var phone = query.Phone.Trim();
            q = q.Where(o => o.Phone.Contains(phone));
        }

        if (query.From.HasValue)
            q = q.Where(o => o.CreatedAt >= query.From.Value);

        if (query.To.HasValue)
        {
            // To la "den het ngay do"
            var to = query.To.Value.Date.AddDays(1);
            q = q.Where(o => o.CreatedAt < to);
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => ToDto(o))
            .ToListAsync(ct);

        return ServiceResult<PagedResult<AdminOrderDto>>.Ok(
            new PagedResult<AdminOrderDto>(items, total, page, pageSize));
    }

    public async Task<ServiceResult<AdminOrderDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);
        return order is null
            ? ServiceResult<AdminOrderDto>.Fail(ServiceErrorCode.NotFound, "Khong tim thay don hang.")
            : ServiceResult<AdminOrderDto>.Ok(ToDto(order));
    }

    public async Task<ServiceResult<AdminOrderDto>> UpdateStatusAsync(
        int id, UpdateOrderStatusRequest req, CancellationToken ct = default)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
            return ServiceResult<AdminOrderDto>.Fail(ServiceErrorCode.NotFound, "Khong tim thay don hang.");

        // AC-27: chan moi buoc chuyen sai so do trang thai.
        if (!OrderStateMachine.CanTransition(order.Status, req.Status))
        {
            return ServiceResult<AdminOrderDto>.Invalid(
                nameof(req.Status),
                $"Khong the chuyen tu {order.Status} sang {req.Status}.");
        }

        order.Status = req.Status;
        order.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);

        return ServiceResult<AdminOrderDto>.Ok(ToDto(order));
    }
}
