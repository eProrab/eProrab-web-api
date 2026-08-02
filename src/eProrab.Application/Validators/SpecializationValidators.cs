using eProrab.Application.DTOs.Specializations;
using FluentValidation;

namespace eProrab.Application.Validators;

public class CreateSpecializationRequestValidator : AbstractValidator<CreateSpecializationRequest>
{
    public CreateSpecializationRequestValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(80)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug must be lowercase, kebab-case (e.g. 'crane-operator').");
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);
    }
}

public class UpdateSpecializationRequestValidator : AbstractValidator<UpdateSpecializationRequest>
{
    public UpdateSpecializationRequestValidator()
    {
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);
    }
}
