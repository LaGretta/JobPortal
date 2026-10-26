using FluentValidation;
using JobPortal.Application.DTO.Applications;
using JobPortal.Domain.Enums;

namespace JobPortal.Application.Validator;

public class CreateApplicationDtoValidator : AbstractValidator<CreateApplicationDto>
{
    public CreateApplicationDtoValidator()
    {
        RuleFor(x => x.VacancyId).NotEmpty();
        RuleFor(x => x.CoverLetter).MaximumLength(2000);
    }
}
public class UpdateApplicationStatusDtoValidator : AbstractValidator<UpdateApplicationStatusDto>
{
    public UpdateApplicationStatusDtoValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Status)
            .NotEqual(ApplicationStatus.Pending)
            .WithMessage("Не можна повернути відгук у початковий стан");
    }
}