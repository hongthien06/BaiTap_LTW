using Microsoft.EntityFrameworkCore;
using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Dtos.Admin;

namespace NhaGiaKim.Application.Services;

public interface IAdminFeedbackService
{
    Task<ServiceResult<PagedResult<AdminFeedbackDto>>> SearchAsync(bool? isApproved, int page, int pageSize, CancellationToken ct = default);
    Task<ServiceResult<AdminFeedbackDto>> SetApprovalAsync(int id, bool approved, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default);
}

public class AdminFeedbackService(IAppDbContext db, TimeProvider clock) : IAdminFeedbackService
{
    public const int MaxPageSize = 100;

    private static AdminFeedbackDto ToDto(Domain.Entities.Feedback f) =>
        new(f.Id, f.CustomerName, f.Rating, f.Content, f.IsApproved, f.CreatedAt, f.ApprovedAt);

    public async Task<ServiceResult<PagedResult<AdminFeedbackDto>>> SearchAsync(
        bool? isApproved, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > MaxPageSize ? 10 : pageSize;

        var q = db.Feedbacks.AsNoTracking().AsQueryable();
        if (isApproved.HasValue)
            q = q.Where(f => f.IsApproved == isApproved.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => ToDto(f))
            .ToListAsync(ct);

        return ServiceResult<PagedResult<AdminFeedbackDto>>.Ok(
            new PagedResult<AdminFeedbackDto>(items, total, page, pageSize));
    }

    public async Task<ServiceResult<AdminFeedbackDto>> SetApprovalAsync(
        int id, bool approved, CancellationToken ct = default)
    {
        var f = await db.Feedbacks.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (f is null)
            return ServiceResult<AdminFeedbackDto>.Fail(ServiceErrorCode.NotFound, "Khong tim thay danh gia.");

        f.IsApproved = approved;
        f.ApprovedAt = approved ? clock.GetUtcNow().UtcDateTime : null;
        await db.SaveChangesAsync(ct);

        return ServiceResult<AdminFeedbackDto>.Ok(ToDto(f));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var f = await db.Feedbacks.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (f is null) return ServiceResult<bool>.Fail(ServiceErrorCode.NotFound, "Khong tim thay danh gia.");

        db.Feedbacks.Remove(f);
        await db.SaveChangesAsync(ct);
        return ServiceResult<bool>.Ok(true);
    }
}
