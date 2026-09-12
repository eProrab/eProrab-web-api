using eProrab.Application.DTOs.Jobs;
using FluentValidation;

namespace eProrab.Application.Validators;

public class CreateJobPostingRequestValidator : AbstractValidator<CreateJobPostingRequest>
{
    public CreateJobPostingRequestValidator()
    {
        RuleFor(x => x.SpecializationId)
            .GreaterThan(0).WithMessage("Specialization ID must be a positive number (greater than 0).");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(150).WithMessage("Job title must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Job description is required.")
            .MaximumLength(4000).WithMessage("Job description must not exceed 4000 characters.");

        RuleFor(x => x.Language)
            .IsInEnum().WithMessage("Language must be a valid enum value.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.BudgetType)
            .IsInEnum().WithMessage("Budget type must be a valid enum value.");

        RuleFor(x => x.BudgetMin)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum budget must be zero or positive.")
            .When(x => x.BudgetMin.HasValue);

        RuleFor(x => x.BudgetMax)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum budget must be zero or positive.")
            .When(x => x.BudgetMax.HasValue);

        RuleFor(x => x)
            .Must(x => !x.BudgetMin.HasValue || !x.BudgetMax.HasValue || x.BudgetMin <= x.BudgetMax)
            .WithMessage("Minimum budget must be less than or equal to maximum budget.")
            .WithName("Budget");

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Duration must be greater than zero (minimum 1 day).")
            .LessThanOrEqualTo(36500).WithMessage("Duration must not exceed 100 years.")
            .When(x => x.DurationDays.HasValue);
    }
}

public class UpdateJobPostingRequestValidator : AbstractValidator<UpdateJobPostingRequest>
{
    public UpdateJobPostingRequestValidator()
    {
        RuleFor(x => x.SpecializationId)
            .GreaterThan(0).WithMessage("Specialization ID must be a positive number (greater than 0).");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(150).WithMessage("Job title must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Job description is required.")
            .MaximumLength(4000).WithMessage("Job description must not exceed 4000 characters.");

        RuleFor(x => x.Language)
            .IsInEnum().WithMessage("Language must be a valid enum value.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.BudgetType)
            .IsInEnum().WithMessage("Budget type must be a valid enum value.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid enum value.");

        RuleFor(x => x.BudgetMin)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum budget must be zero or positive.")
            .When(x => x.BudgetMin.HasValue);

        RuleFor(x => x.BudgetMax)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum budget must be zero or positive.")
            .When(x => x.BudgetMax.HasValue);

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Duration must be greater than zero (minimum 1 day).")
            .LessThanOrEqualTo(36500).WithMessage("Duration must not exceed 100 years.")
            .When(x => x.DurationDays.HasValue);
    }
}

public class CreateJobApplicationRequestValidator : AbstractValidator<CreateJobApplicationRequest>
{
    public CreateJobApplicationRequestValidator()
    {
        RuleFor(x => x.CoverMessage)
            .MaximumLength(2000).WithMessage("Cover message must not exceed 2000 characters.");

        RuleFor(x => x.ProposedRate)
            .GreaterThanOrEqualTo(0).WithMessage("Proposed rate must be zero or positive.")
            .LessThan(decimal.MaxValue).WithMessage("Proposed rate is too large.")
            .When(x => x.ProposedRate.HasValue);
    }
}

public class AcceptJobApplicationRequestValidator : AbstractValidator<AcceptJobApplicationRequest>
{
    public AcceptJobApplicationRequestValidator()
    {
        RuleFor(x => x.AgreedRate)
            .GreaterThanOrEqualTo(0).WithMessage("Agreed rate must be zero or positive.")
            .LessThan(decimal.MaxValue).WithMessage("Agreed rate is too large.")
            .When(x => x.AgreedRate.HasValue);
    }
}
