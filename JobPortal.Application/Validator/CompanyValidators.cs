using FluentValidation;
using JobPortal.Application.DTO.Companies;

namespace JobPortal.Application.Validator;

public class UpdateCompanyDtoValidator : AbstractValidator<UpdateCompanyDto>
{
    public UpdateCompanyDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.LogoUrl).MaximumLength(500);

        RuleFor(x => x.Website)
            .MaximumLength(200)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var u)
                         && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps))
            .When(x => !string.IsNullOrWhiteSpace(x.Website))
            .WithMessage("Вкажіть повну адресу, напр. https://example.com");
    }
}