using eProrab.Application.DTOs.Workers;
using FluentValidation;

namespace eProrab.Application.Validators;

public class UpsertWorkerProfileRequestValidator : AbstractValidator<UpsertWorkerProfileRequest>
{
    public UpsertWorkerProfileRequestValidator()
    {
        RuleFor(x => x.SpecializationId)
            .GreaterThan(0).WithMessage("Specialization ID must be a positive number (greater than 0).");

        RuleFor(x => x.ExperienceYears)
            .InclusiveBetween(0, 60).WithMessage("Experience years must be between 0 and 60 years.");

        RuleFor(x => x.Bio)
            .MaximumLength(2000).WithMessage("Bio must not exceed 2000 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.DailyRate)
            .GreaterThanOrEqualTo(0).WithMessage("Daily rate must be zero or positive.")
            .LessThan(decimal.MaxValue).WithMessage("Daily rate is too large.")
            .When(x => x.DailyRate.HasValue);
    }
}
