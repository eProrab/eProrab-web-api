using eProrab.Application.DTOs.Jobs;
using FluentValidation;

namespace eProrab.Application.Validators;

public class CreateJobPostingRequestValidator : AbstractValidator<CreateJobPostingRequest>
{
    public CreateJobPostingRequestValidator()
    {
        RuleFor(x => x.SpecializationId)
            .GreaterThan(0).WithMessage("Mütəxəssis sahəsi ID müsbət rəqəm olmalıdır (0-dan böyük).");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("İş adı tələb olunur.")
            .MaximumLength(150).WithMessage("İş adı maksimum 150 simvoldan çox ola bilməz.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("İş təsviri tələb olunur.")
            .MaximumLength(4000).WithMessage("İş təsviri maksimum 4000 simvoldan çox ola bilməz.");

        RuleFor(x => x.Language)
            .IsInEnum().WithMessage("Dil seçimi düzgün deyil.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Şəhər maksimum 100 simvoldan çox ola bilməz.");

        RuleFor(x => x.BudgetType)
            .IsInEnum().WithMessage("Büdcə tipi düzgün deyil.");

        RuleFor(x => x.BudgetMin)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum büdcə sıfır və ya müsbət olmalıdır.")
            .When(x => x.BudgetMin.HasValue);

        RuleFor(x => x.BudgetMax)
            .GreaterThanOrEqualTo(0).WithMessage("Maksimum büdcə sıfır və ya müsbət olmalıdır.")
            .When(x => x.BudgetMax.HasValue);

        RuleFor(x => x)
            .Must(x => !x.BudgetMin.HasValue || !x.BudgetMax.HasValue || x.BudgetMin <= x.BudgetMax)
            .WithMessage("Minimum büdcə maksimum büdcədən az və ya bərabər olmalıdır.")
            .WithName("Büdcə");

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Müddət sıfırdan böyük olmalıdır (minimum 1 gün).")
            .LessThanOrEqualTo(36500).WithMessage("Müddət 100 ildən çox ola bilməz.")
            .When(x => x.DurationDays.HasValue);
    }
}

public class UpdateJobPostingRequestValidator : AbstractValidator<UpdateJobPostingRequest>
{
    public UpdateJobPostingRequestValidator()
    {
        RuleFor(x => x.SpecializationId)
            .GreaterThan(0).WithMessage("Mütəxəssis sahəsi ID müsbət rəqəm olmalıdır (0-dan böyük).");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("İş adı tələb olunur.")
            .MaximumLength(150).WithMessage("İş adı maksimum 150 simvoldan çox ola bilməz.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("İş təsviri tələb olunur.")
            .MaximumLength(4000).WithMessage("İş təsviri maksimum 4000 simvoldan çox ola bilməz.");

        RuleFor(x => x.Language)
            .IsInEnum().WithMessage("Dil seçimi düzgün deyil.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Şəhər maksimum 100 simvoldan çox ola bilməz.");

        RuleFor(x => x.BudgetType)
            .IsInEnum().WithMessage("Büdcə tipi düzgün deyil.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status düzgün deyil.");

        RuleFor(x => x.BudgetMin)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum büdcə sıfır və ya müsbət olmalıdır.")
            .When(x => x.BudgetMin.HasValue);

        RuleFor(x => x.BudgetMax)
            .GreaterThanOrEqualTo(0).WithMessage("Maksimum büdcə sıfır və ya müsbət olmalıdır.")
            .When(x => x.BudgetMax.HasValue);

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Müddət sıfırdan böyük olmalıdır (minimum 1 gün).")
            .LessThanOrEqualTo(36500).WithMessage("Müddət 100 ildən çox ola bilməz.")
            .When(x => x.DurationDays.HasValue);
    }
}

public class CreateJobApplicationRequestValidator : AbstractValidator<CreateJobApplicationRequest>
{
    public CreateJobApplicationRequestValidator()
    {
        RuleFor(x => x.CoverMessage)
            .MaximumLength(2000).WithMessage("Müraciət məktəbi maksimum 2000 simvoldan çox ola bilməz.");

        RuleFor(x => x.ProposedRate)
            .GreaterThanOrEqualTo(0).WithMessage("Təklif olunan tarif sıfır və ya müsbət olmalıdır.")
            .LessThan(decimal.MaxValue).WithMessage("Təklif olunan tarif çox böyükdür.")
            .When(x => x.ProposedRate.HasValue);
    }
}

public class AcceptJobApplicationRequestValidator : AbstractValidator<AcceptJobApplicationRequest>
{
    public AcceptJobApplicationRequestValidator()
    {
        RuleFor(x => x.AgreedRate)
            .GreaterThanOrEqualTo(0).WithMessage("Razı gəlinən tarif sıfır və ya müsbət olmalıdır.")
            .LessThan(decimal.MaxValue).WithMessage("Razı gəlinən tarif çox böyükdür.")
            .When(x => x.AgreedRate.HasValue);
    }
}
