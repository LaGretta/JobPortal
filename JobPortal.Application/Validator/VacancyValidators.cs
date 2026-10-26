using FluentValidation;
using JobPortal.Application.DTO.Vacancies;

namespace JobPortal.Application.Validator;

public class CreateVacancyDtoValidator : AbstractValidator<CreateVacancyDto>
{
    public CreateVacancyDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(3).MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(20).MaximumLength(5000);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);

        RuleFor(x => x.EmploymentType).IsInEnum();
        RuleFor(x => x.WorkMode).IsInEnum();
        RuleFor(x => x.Currency).IsInEnum();

        RuleFor(x => x.SalaryMin).GreaterThan(0).When(x => x.SalaryMin.HasValue);
        RuleFor(x => x.SalaryMax).GreaterThan(0).When(x => x.SalaryMax.HasValue);

        RuleFor(x => x.SalaryMax)
            .GreaterThanOrEqualTo(x => x.SalaryMin!.Value)
            .When(x => x.SalaryMin.HasValue && x.SalaryMax.HasValue)
            .WithMessage("Максимум не може бути меншим за мінімум");
    }
}

public class UpdateVacancyDtoValidator : AbstractValidator<UpdateVacancyDto>
{
    public UpdateVacancyDtoValidator()
    {
        Include(new CreateVacancyDtoValidator());
    }
}