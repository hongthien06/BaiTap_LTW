using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Dtos.Public;
using NhaGiaKim.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace NhaGiaKim.Application.Services;

public interface IFeedbackService
{
    Task<ServiceResult<CreateFeedbackResponse>> CreateAsync(CreateFeedbackRequest request, CancellationToken ct = default);
}

public class FeedbackService(IAppDbContext db, TimeProvider clock) : IFeedbackService
{
    public async Task<ServiceResult<CreateFeedbackResponse>> CreateAsync(
        CreateFeedbackRequest request, CancellationToken ct = default)
    {
        var bookId = await db.Books.Where(b => b.IsActive)
            .OrderBy(b => b.Id)
            .Select(b => (int?)b.Id)
            .FirstOrDefaultAsync(ct);

        if (bookId is null)
        {
            return ServiceResult<CreateFeedbackResponse>.Fail(
                ServiceErrorCode.NotFound, "Khong tim thay sach dang mo ban.");
        }

        var feedback = new Feedback
        {
            BookId = bookId.Value,
            CustomerName = request.CustomerName.Trim(),
            Rating = request.Rating,
            Content = request.Content.Trim(),
            IsApproved = false,            // AC-13: luon cho duyet
            CreatedAt = clock.GetUtcNow().UtcDateTime
        };

        db.Feedbacks.Add(feedback);
        await db.SaveChangesAsync(ct);

        return ServiceResult<CreateFeedbackResponse>.Ok(new CreateFeedbackResponse(
            feedback.Id, "Cam on ban! Danh gia se hien thi sau khi duoc duyet."));
    }
}
