using FluentValidation;
using NhaGiaKim.Application.Dtos.Public;

namespace NhaGiaKim.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    /// <summary>SDT Viet Nam: bat dau bang 0, tong 10 chu so.</summary>
    public const string PhonePattern = @"^0\d{9}$";

    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Vui long nhap ho ten.")
            .MinimumLength(2).WithMessage("Ho ten phai co it nhat 2 ky tu.")
            .MaximumLength(200).WithMessage("Ho ten toi da 200 ky tu.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Vui long nhap so dien thoai.")
            .Matches(PhonePattern).WithMessage("So dien thoai khong hop le (10 so, bat dau bang 0).");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Vui long nhap dia chi.")
            .MinimumLength(10).WithMessage("Dia chi phai co it nhat 10 ky tu.")
            .MaximumLength(500).WithMessage("Dia chi toi da 500 ky tu.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 99).WithMessage("So luong phai tu 1 den 99.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Phuong thuc thanh toan khong hop le.");

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Ghi chu toi da 1000 ky tu.");
    }
}
