using eProrab.Application.DTOs.Specializations;
using FluentValidation;

namespace eProrab.Application.Validators;

public class CreateSpecializationRequestValidator : AbstractValidator<CreateSpecializationRequest>
{
    public CreateSpecializationRequestValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("URL adı tələb olunur.")
            .MaximumLength(80).WithMessage("URL adı maksimum 80 simvoldan çox ola bilməz.")
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("URL adı yalnız kiçik hərflər, rəqəmlər və tire ilə olmalıdır (məs. 'qar-operatoru').");
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Göstəriş sırası 0 və ya müsbət olmalıdır.");
        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);
    }
}

public class UpdateSpecializationRequestValidator : AbstractValidator<UpdateSpecializationRequest>
{
    public UpdateSpecializationRequestValidator()
    {
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Göstəriş sırası 0 və ya müsbət olmalıdır.");
        RuleFor(x => x.Translations)
            .MustCoverAllLanguages(t => t.Language, t => t.Name);
    }
}
