using FluentValidation;
using NhaGiaKim.Application.Dtos.Admin;

namespace NhaGiaKim.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(100);
    }
}
