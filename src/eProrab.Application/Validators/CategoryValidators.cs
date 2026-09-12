using eProrab.Application.DTOs.Categories;
using FluentValidation;

namespace eProrab.Application.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("URL adı tələb olunur.")
            .MaximumLength(80).WithMessage("URL adı maksimum 80 simvoldan çox ola bilməz.")
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("URL adı yalnız kiçik hərflər, rəqəmlər və tire ilə olmalıdır (məs. 'əl-aləti').");
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Göstəriş sırası 0 və ya müsbət olmalıdır.");
        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);
    }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Göstəriş sırası 0 və ya müsbət olmalıdır.");
        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);
    }
}
