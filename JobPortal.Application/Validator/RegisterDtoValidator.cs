using FluentValidation;
using JobPortal.Application.DTO;
using JobPortal.Domain.Enums;

namespace JobPortal.Application.Validator;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);

        RuleFor(x => x.Role)
            .Must(r => r == UserRole.JobSeeker || r == UserRole.Employer)
            .WithMessage("Тільки здобувач або роботодавець");
    }
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
    public class RefreshDtoValidator : AbstractValidator<RefreshDto>
    {
        public RefreshDtoValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }
}