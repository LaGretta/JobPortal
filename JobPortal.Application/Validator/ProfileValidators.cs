using FluentValidation;
using JobPortal.Application.DTO.Profiles;

namespace JobPortal.Application.Validator;

public class UpdateCandidateProfileDtoValidator : AbstractValidator<UpdateCandidateProfileDto>
{
    public UpdateCandidateProfileDtoValidator()
    {
        RuleFor(x => x.Headline).MaximumLength(200);
        RuleFor(x => x.About).MaximumLength(2000);
    }
}