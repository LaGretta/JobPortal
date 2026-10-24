using FluentValidation;
using JobPortal.Application.DTO.Users;

namespace JobPortal.Application.Validator;

public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MinimumLength(3).MaximumLength(50);
    }
}