using eProrab.Application.DTOs.Workers;
using FluentValidation;

namespace eProrab.Application.Validators;

public class UpsertWorkerProfileRequestValidator : AbstractValidator<UpsertWorkerProfileRequest>
{
    public UpsertWorkerProfileRequestValidator()
    {
        RuleFor(x => x.SpecializationId)
            .GreaterThan(0).WithMessage("Mütəxəssis sahəsi ID müsbət rəqəm olmalıdır (0-dan böyük).");

        RuleFor(x => x.ExperienceYears)
            .InclusiveBetween(0, 60).WithMessage("Təcrübə illəri 0 ilə 60 arasında olmalıdır.");

        RuleFor(x => x.Bio)
            .MaximumLength(2000).WithMessage("Bioqrafiya maksimum 2000 simvoldan çox ola bilməz.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Şəhər maksimum 100 simvoldan çox ola bilməz.");

        RuleFor(x => x.DailyRate)
            .GreaterThanOrEqualTo(0).WithMessage("Gündəlik tarif sıfır və ya müsbət olmalıdır.")
            .LessThan(decimal.MaxValue).WithMessage("Gündəlik tarif çox böyükdür.")
            .When(x => x.DailyRate.HasValue);
    }
}
