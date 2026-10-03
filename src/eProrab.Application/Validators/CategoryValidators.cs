using eProrab.Application.DTOs.Categories;
using FluentValidation;

namespace eProrab.Application.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(80)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug must be lowercase, kebab-case (e.g. 'hand-tools').");
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);
    }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Slug)
            .MaximumLength(80)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug must be lowercase, kebab-case (e.g. 'hand-tools').")
            .When(x => !string.IsNullOrEmpty(x.Slug));
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);
    }
}
