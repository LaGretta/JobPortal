using FluentValidation;
using JobPortal.Application.DTO.Auth;

namespace JobPortal.Application.Validator;

public class RegisterCandidateDtoValidator : AbstractValidator<RegisterCandidateDto>
{
    public RegisterCandidateDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
    }
}

public class RegisterEmployerDtoValidator : AbstractValidator<RegisterEmployerDto>
{
    public RegisterEmployerDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);

        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CompanyDescription).MaximumLength(2000);

        RuleFor(x => x.CompanyWebsite)
            .MaximumLength(200)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var u)
                         && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps))
            .When(x => !string.IsNullOrWhiteSpace(x.CompanyWebsite))
            .WithMessage("Вкажіть повну адресу, напр. https://example.com");
    }
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