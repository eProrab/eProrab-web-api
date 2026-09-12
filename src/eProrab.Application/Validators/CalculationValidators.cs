using eProrab.Application.DTOs.Calculations;
using FluentValidation;

namespace eProrab.Application.Validators;

public class SaveCalculationRequestValidator : AbstractValidator<SaveCalculationRequest>
{
    // Valid enum-like values for these fields
    private static readonly string[] ValidPropertyTypes = { "apartment", "house", "office", "commercial", "other" };
    private static readonly string[] ValidStructureAges = { "new", "1-5", "5-10", "10-20", "20-30", "30+" };
    private static readonly string[] ValidRepairStyles = { "basic", "standard", "premium", "luxury" };
    private static readonly string[] ValidTariffTiers = { "tier1", "tier2", "tier3", "tier4" };

    public SaveCalculationRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Başlıq maksimum 200 simvoldan çox ola bilməz.")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.PropertyType)
            .NotEmpty().WithMessage("Mülk tipi tələb olunur.")
            .MaximumLength(50).WithMessage("Mülk tipi maksimum 50 simvoldan çox ola bilməz.")
            .Must(BeValidPropertyType).WithMessage("Mülk tipi aşağıdakılardan biri olmalıdır: apartment, house, office, commercial, other.");

        RuleFor(x => x.StructureAge)
            .NotEmpty().WithMessage("Binanın yaşı tələb olunur.")
            .MaximumLength(50).WithMessage("Binanın yaşı maksimum 50 simvoldan çox ola bilməz.")
            .Must(BeValidStructureAge).WithMessage("Binanın yaşı aşağıdakılardan biri olmalıdır: new, 1-5, 5-10, 10-20, 20-30, 30+.");

        RuleFor(x => x.RepairStyle)
            .NotEmpty().WithMessage("Təmir stili tələb olunur.")
            .MaximumLength(50).WithMessage("Təmir stili maksimum 50 simvoldan çox ola bilməz.")
            .Must(BeValidRepairStyle).WithMessage("Təmir stili aşağıdakılardan biri olmalıdır: basic, standard, premium, luxury.");

        RuleFor(x => x.TariffTier)
            .NotEmpty().WithMessage("Tarif səviyyəsi tələb olunur.")
            .MaximumLength(50).WithMessage("Tarif səviyyəsi maksimum 50 simvoldan çox ola bilməz.")
            .Must(BeValidTariffTier).WithMessage("Tarif səviyyəsi aşağıdakılardan biri olmalıdır: tier1, tier2, tier3, tier4.");

        RuleFor(x => x.TotalArea)
            .GreaterThan(0).WithMessage("Ümumi sahə sıfırdan böyük olmalıdır.")
            .LessThanOrEqualTo(10000).WithMessage("Ümumi sahə 10,000 kvadrat metrə qədər olmalıdır.");

        RuleFor(x => x.WallHeight)
            .GreaterThan(0).WithMessage("Divar hündürlüyü sıfırdan böyük olmalıdır.")
            .LessThanOrEqualTo(10).WithMessage("Divar hündürlüyü 10 metrə qədər olmalıdır.");

        RuleFor(x => x.RoomCount)
            .GreaterThanOrEqualTo(0).WithMessage("Otaq sayı sıfır və ya müsbət olmalıdır.")
            .LessThanOrEqualTo(1000).WithMessage("Otaq sayı 1,000-ə qədər olmalıdır.");

        RuleFor(x => x.DoorCount)
            .GreaterThanOrEqualTo(0).WithMessage("Qapı sayı sıfır və ya müsbət olmalıdır.")
            .LessThanOrEqualTo(1000).WithMessage("Qapı sayı 1,000-ə qədər olmalıdır.");

        RuleFor(x => x.WindowCount)
            .GreaterThanOrEqualTo(0).WithMessage("Pəncərə sayı sıfır və ya müsbət olmalıdır.")
            .LessThanOrEqualTo(1000).WithMessage("Pəncərə sayı 1,000-ə qədər olmalıdır.");

        RuleFor(x => x.RoomsJson)
            .MaximumLength(50000).WithMessage("Otaqlar JSON maksimum 50,000 simvoldan çox ola bilməz.")
            .When(x => !string.IsNullOrWhiteSpace(x.RoomsJson));

        RuleFor(x => x.RoomsInput)
            .NotNull().When(x => x.RoomsJson == null)
            .Custom((rooms, context) =>
            {
                if (rooms == null) return;

                for (int i = 0; i < rooms.Count; i++)
                {
                    if (rooms[i].Area <= 0)
                    {
                        context.AddFailure($"RoomsInput[{i}].Area", "Otaq sahəsi sıfırdan böyük olmalıdır.");
                    }
                    if (rooms[i].Area > 10000)
                    {
                        context.AddFailure($"RoomsInput[{i}].Area", "Otaq sahəsi 10,000 kvadrat metrə qədər olmalıdır.");
                    }
                }
            });
    }

    private bool BeValidPropertyType(string value) =>
        ValidPropertyTypes.Contains(value?.ToLowerInvariant() ?? "");

    private bool BeValidStructureAge(string value) =>
        ValidStructureAges.Contains(value?.ToLowerInvariant() ?? "");

    private bool BeValidRepairStyle(string value) =>
        ValidRepairStyles.Contains(value?.ToLowerInvariant() ?? "");

    private bool BeValidTariffTier(string value) =>
        ValidTariffTiers.Contains(value?.ToLowerInvariant() ?? "");
}

public class CalculationEstimateRequestValidator : AbstractValidator<CalculationEstimateRequest>
{
    // Valid enum-like values for these fields
    private static readonly string[] ValidPropertyTypes = { "apartment", "house", "office", "commercial", "other" };
    private static readonly string[] ValidStructureAges = { "new", "1-5", "5-10", "10-20", "20-30", "30+" };
    private static readonly string[] ValidRepairStyles = { "basic", "standard", "premium", "luxury" };
    private static readonly string[] ValidTariffTiers = { "tier1", "tier2", "tier3", "tier4" };

    public CalculationEstimateRequestValidator()
    {
        RuleFor(x => x.PropertyType)
            .NotEmpty().WithMessage("Mülk tipi tələb olunur.")
            .MaximumLength(50).WithMessage("Mülk tipi maksimum 50 simvoldan çox ola bilməz.")
            .Must(BeValidPropertyType).WithMessage("Mülk tipi aşağıdakılardan biri olmalıdır: apartment, house, office, commercial, other.");

        RuleFor(x => x.StructureAge)
            .NotEmpty().WithMessage("Binanın yaşı tələb olunur.")
            .MaximumLength(50).WithMessage("Binanın yaşı maksimum 50 simvoldan çox ola bilməz.")
            .Must(BeValidStructureAge).WithMessage("Binanın yaşı aşağıdakılardan biri olmalıdır: new, 1-5, 5-10, 10-20, 20-30, 30+.");

        RuleFor(x => x.RepairStyle)
            .NotEmpty().WithMessage("Təmir stili tələb olunur.")
            .MaximumLength(50).WithMessage("Təmir stili maksimum 50 simvoldan çox ola bilməz.")
            .Must(BeValidRepairStyle).WithMessage("Təmir stili aşağıdakılardan biri olmalıdır: basic, standard, premium, luxury.");

        RuleFor(x => x.TariffTier)
            .NotEmpty().WithMessage("Tarif səviyyəsi tələb olunur.")
            .MaximumLength(50).WithMessage("Tarif səviyyəsi maksimum 50 simvoldan çox ola bilməz.")
            .Must(BeValidTariffTier).WithMessage("Tarif səviyyəsi aşağıdakılardan biri olmalıdır: tier1, tier2, tier3, tier4.");

        RuleFor(x => x.TotalArea)
            .GreaterThan(0).WithMessage("Ümumi sahə sıfırdan böyük olmalıdır.")
            .LessThanOrEqualTo(10000).WithMessage("Ümumi sahə 10,000 kvadrat metrə qədər olmalıdır.");

        RuleFor(x => x.RoomsJson)
            .MaximumLength(50000).WithMessage("Otaqlar JSON maksimum 50,000 simvoldan çox ola bilməz.")
            .When(x => !string.IsNullOrWhiteSpace(x.RoomsJson));

        RuleFor(x => x.Rooms)
            .Custom((rooms, context) =>
            {
                if (rooms == null) return;

                if (rooms.Count > 1000)
                {
                    context.AddFailure("Rooms", "1,000-dən çox otaq ola bilməz.");
                    return;
                }

                for (int i = 0; i < rooms.Count; i++)
                {
                    if (rooms[i].Area <= 0)
                    {
                        context.AddFailure($"Rooms[{i}].Area", "Otaq sahəsi sıfırdan böyük olmalıdır.");
                    }
                    if (rooms[i].Area > 10000)
                    {
                        context.AddFailure($"Rooms[{i}].Area", "Otaq sahəsi 10,000 kvadrat metrə qədər olmalıdır.");
                    }
                }
            });
    }

    private bool BeValidPropertyType(string value) =>
        ValidPropertyTypes.Contains(value?.ToLowerInvariant() ?? "");

    private bool BeValidStructureAge(string value) =>
        ValidStructureAges.Contains(value?.ToLowerInvariant() ?? "");

    private bool BeValidRepairStyle(string value) =>
        ValidRepairStyles.Contains(value?.ToLowerInvariant() ?? "");

    private bool BeValidTariffTier(string value) =>
        ValidTariffTiers.Contains(value?.ToLowerInvariant() ?? "");
}
