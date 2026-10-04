using FluentValidation;
using NhaGiaKim.Application.Dtos.Public;

namespace NhaGiaKim.Application.Validators;

public class CreateFeedbackRequestValidator : AbstractValidator<CreateFeedbackRequest>
{
    public CreateFeedbackRequestValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Vui long nhap ten.")
            .MinimumLength(2).WithMessage("Ten phai co it nhat 2 ky tu.")
            .MaximumLength(200).WithMessage("Ten toi da 200 ky tu.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("So sao phai tu 1 den 5.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Vui long nhap noi dung danh gia.")
            .MaximumLength(2000).WithMessage("Noi dung toi da 2000 ky tu.");
    }
}
