using eProrab.Application.DTOs.Workers;
using FluentValidation;

namespace eProrab.Application.Validators;

public class UpsertWorkerProfileRequestValidator : AbstractValidator<UpsertWorkerProfileRequest>
{
    public UpsertWorkerProfileRequestValidator()
    {
        RuleFor(x => x.SpecializationId).GreaterThan(0);
        RuleFor(x => x.ExperienceYears).InclusiveBetween(0, 60);
        RuleFor(x => x.Bio).MaximumLength(2000);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.DailyRate).GreaterThanOrEqualTo(0).When(x => x.DailyRate.HasValue);
    }
}
