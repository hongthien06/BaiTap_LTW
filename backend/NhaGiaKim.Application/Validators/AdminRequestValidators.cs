using FluentValidation;
using NhaGiaKim.Application.Common;
using NhaGiaKim.Application.Dtos.Admin;
using NhaGiaKim.Domain.Enums;

namespace NhaGiaKim.Application.Validators;

// NFR-3: backend la nguon chan ly. Khong co validator thi DTO admin la positional record voi
// string non-nullable khong "required", nen JSON thieu field se thanh null -> .Trim() nem
// NullReferenceException -> 500 thay vi 400. Gioi han do dai phai khop cau hinh EF,
// neu khong thi chuoi qua dai se thanh DbUpdateException -> cung 500.

public class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    public UpdateBookRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui long nhap ten sach.").MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().WithMessage("Vui long nhap the loai.").MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().WithMessage("Vui long nhap title.").MaximumLength(300);
        RuleFor(x => x.Subtitle).NotNull().MaximumLength(500);
        RuleFor(x => x.Description).NotNull();
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Gia phai lon hon 0.");
        RuleFor(x => x.DiscountPrice)
            .GreaterThan(0).LessThan(x => x.Price)
            .WithMessage("Gia giam phai lon hon 0 va nho hon gia goc.")
            .When(x => x.DiscountPrice.HasValue);
        RuleFor(x => x.CoverImageUrl).MaximumLength(500);
        RuleFor(x => x.MockupImageUrl).MaximumLength(500);
    }
}

public class UpdateAuthorRequestValidator : AbstractValidator<UpdateAuthorRequest>
{
    public UpdateAuthorRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Vui long nhap ho ten tac gia.").MaximumLength(200);
        RuleFor(x => x.AvatarUrl).MaximumLength(500);
        RuleFor(x => x.Bio).NotNull();
    }
}

public class PressQuoteRequestValidator : AbstractValidator<PressQuoteRequest>
{
    public PressQuoteRequestValidator()
    {
        RuleFor(x => x.PressName).NotEmpty().WithMessage("Vui long nhap ten bao.").MaximumLength(200);
        RuleFor(x => x.Quote).NotEmpty().WithMessage("Vui long nhap trich doan.").MaximumLength(1000);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
        RuleFor(x => x.SourceUrl).MaximumLength(500);
        RuleFor(x => x.SourceUrl).Must(UrlRules.IsSafeLink)
            .WithMessage("Link khong hop le (chi chap nhan http, https hoac duong dan noi bo).")
            .When(x => !string.IsNullOrWhiteSpace(x.SourceUrl));
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateReviewRequestValidator : AbstractValidator<UpdateReviewRequest>
{
    public UpdateReviewRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Vui long nhap tieu de.").MaximumLength(300);
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.FileUrl).MaximumLength(500);
        RuleFor(x => x.FileUrl).Must(UrlRules.IsSafeLink)
            .WithMessage("Duong dan file khong hop le.")
            .When(x => !string.IsNullOrWhiteSpace(x.FileUrl));
    }
}

public class UpdateSettingsRequestValidator : AbstractValidator<UpdateSettingsRequest>
{
    public UpdateSettingsRequestValidator()
    {
        RuleFor(x => x.Items).NotNull();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Key).NotEmpty().MaximumLength(100);
            item.RuleFor(i => i.Value).NotNull().MaximumLength(1000);
            item.RuleFor(i => i.Description).MaximumLength(300);
        });
    }
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Vui long nhap email.")
            .EmailAddress().WithMessage("Email khong hop le.")
            .MaximumLength(256);
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Vui long nhap ho ten.").MaximumLength(200);
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(8).WithMessage("Mat khau toi thieu 8 ky tu.")
            .MaximumLength(100);
        RuleFor(x => x.Role).Must(IsValidRole).WithMessage("Role khong hop le.");
    }

    internal static bool IsValidRole(string role) => role is RoleName.Admin or RoleName.Staff;
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Vui long nhap ho ten.").MaximumLength(200);
        RuleFor(x => x.Role).Must(CreateUserRequestValidator.IsValidRole).WithMessage("Role khong hop le.");
        RuleFor(x => x.NewPassword)
            .MinimumLength(8).WithMessage("Mat khau toi thieu 8 ky tu.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.NewPassword));
    }
}

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("Trang thai khong hop le.");
    }
}
