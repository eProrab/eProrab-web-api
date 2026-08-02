using eProrab.Application.DTOs.Jobs;
using FluentValidation;

namespace eProrab.Application.Validators;

public class CreateJobPostingRequestValidator : AbstractValidator<CreateJobPostingRequest>
{
    public CreateJobPostingRequestValidator()
    {
        RuleFor(x => x.SpecializationId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Language).IsInEnum();
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.BudgetType).IsInEnum();
        RuleFor(x => x.BudgetMin).GreaterThanOrEqualTo(0).When(x => x.BudgetMin.HasValue);
        RuleFor(x => x.BudgetMax).GreaterThanOrEqualTo(0).When(x => x.BudgetMax.HasValue);
        RuleFor(x => x)
            .Must(x => !x.BudgetMin.HasValue || !x.BudgetMax.HasValue || x.BudgetMin <= x.BudgetMax)
            .WithMessage("BudgetMin must be less than or equal to BudgetMax.")
            .WithName("Budget");
        RuleFor(x => x.DurationDays).GreaterThan(0).When(x => x.DurationDays.HasValue);
    }
}

public class UpdateJobPostingRequestValidator : AbstractValidator<UpdateJobPostingRequest>
{
    public UpdateJobPostingRequestValidator()
    {
        RuleFor(x => x.SpecializationId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Language).IsInEnum();
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.BudgetType).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.BudgetMin).GreaterThanOrEqualTo(0).When(x => x.BudgetMin.HasValue);
        RuleFor(x => x.BudgetMax).GreaterThanOrEqualTo(0).When(x => x.BudgetMax.HasValue);
        RuleFor(x => x.DurationDays).GreaterThan(0).When(x => x.DurationDays.HasValue);
    }
}

public class CreateJobApplicationRequestValidator : AbstractValidator<CreateJobApplicationRequest>
{
    public CreateJobApplicationRequestValidator()
    {
        RuleFor(x => x.CoverMessage).MaximumLength(2000);
        RuleFor(x => x.ProposedRate).GreaterThanOrEqualTo(0).When(x => x.ProposedRate.HasValue);
    }
}

public class AcceptJobApplicationRequestValidator : AbstractValidator<AcceptJobApplicationRequest>
{
    public AcceptJobApplicationRequestValidator()
    {
        RuleFor(x => x.AgreedRate).GreaterThanOrEqualTo(0).When(x => x.AgreedRate.HasValue);
    }
}
