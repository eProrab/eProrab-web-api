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
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.PropertyType)
            .NotEmpty().WithMessage("Property type is required.")
            .MaximumLength(50).WithMessage("Property type must not exceed 50 characters.")
            .Must(BeValidPropertyType).WithMessage("Property type must be one of: apartment, house, office, commercial, other.");

        RuleFor(x => x.StructureAge)
            .NotEmpty().WithMessage("Structure age is required.")
            .MaximumLength(50).WithMessage("Structure age must not exceed 50 characters.")
            .Must(BeValidStructureAge).WithMessage("Structure age must be one of: new, 1-5, 5-10, 10-20, 20-30, 30+.");

        RuleFor(x => x.RepairStyle)
            .NotEmpty().WithMessage("Repair style is required.")
            .MaximumLength(50).WithMessage("Repair style must not exceed 50 characters.")
            .Must(BeValidRepairStyle).WithMessage("Repair style must be one of: basic, standard, premium, luxury.");

        RuleFor(x => x.TariffTier)
            .NotEmpty().WithMessage("Tariff tier is required.")
            .MaximumLength(50).WithMessage("Tariff tier must not exceed 50 characters.")
            .Must(BeValidTariffTier).WithMessage("Tariff tier must be one of: tier1, tier2, tier3, tier4.");

        RuleFor(x => x.TotalArea)
            .GreaterThan(0).WithMessage("Total area must be greater than zero.")
            .LessThanOrEqualTo(10000).WithMessage("Total area must not exceed 10,000 square meters.");

        RuleFor(x => x.WallHeight)
            .GreaterThan(0).WithMessage("Wall height must be greater than zero.")
            .LessThanOrEqualTo(10).WithMessage("Wall height must not exceed 10 meters.");

        RuleFor(x => x.RoomCount)
            .GreaterThanOrEqualTo(0).WithMessage("Room count must be zero or positive.")
            .LessThanOrEqualTo(1000).WithMessage("Room count must not exceed 1,000.");

        RuleFor(x => x.DoorCount)
            .GreaterThanOrEqualTo(0).WithMessage("Door count must be zero or positive.")
            .LessThanOrEqualTo(1000).WithMessage("Door count must not exceed 1,000.");

        RuleFor(x => x.WindowCount)
            .GreaterThanOrEqualTo(0).WithMessage("Window count must be zero or positive.")
            .LessThanOrEqualTo(1000).WithMessage("Window count must not exceed 1,000.");

        RuleFor(x => x.RoomsJson)
            .MaximumLength(50000).WithMessage("Rooms JSON must not exceed 50,000 characters.")
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
                        context.AddFailure($"RoomsInput[{i}].Area", "Room area must be greater than zero.");
                    }
                    if (rooms[i].Area > 10000)
                    {
                        context.AddFailure($"RoomsInput[{i}].Area", "Room area must not exceed 10,000 square meters.");
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
            .NotEmpty().WithMessage("Property type is required.")
            .MaximumLength(50).WithMessage("Property type must not exceed 50 characters.")
            .Must(BeValidPropertyType).WithMessage("Property type must be one of: apartment, house, office, commercial, other.");

        RuleFor(x => x.StructureAge)
            .NotEmpty().WithMessage("Structure age is required.")
            .MaximumLength(50).WithMessage("Structure age must not exceed 50 characters.")
            .Must(BeValidStructureAge).WithMessage("Structure age must be one of: new, 1-5, 5-10, 10-20, 20-30, 30+.");

        RuleFor(x => x.RepairStyle)
            .NotEmpty().WithMessage("Repair style is required.")
            .MaximumLength(50).WithMessage("Repair style must not exceed 50 characters.")
            .Must(BeValidRepairStyle).WithMessage("Repair style must be one of: basic, standard, premium, luxury.");

        RuleFor(x => x.TariffTier)
            .NotEmpty().WithMessage("Tariff tier is required.")
            .MaximumLength(50).WithMessage("Tariff tier must not exceed 50 characters.")
            .Must(BeValidTariffTier).WithMessage("Tariff tier must be one of: tier1, tier2, tier3, tier4.");

        RuleFor(x => x.TotalArea)
            .GreaterThan(0).WithMessage("Total area must be greater than zero.")
            .LessThanOrEqualTo(10000).WithMessage("Total area must not exceed 10,000 square meters.");

        RuleFor(x => x.RoomsJson)
            .MaximumLength(50000).WithMessage("Rooms JSON must not exceed 50,000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RoomsJson));

        RuleFor(x => x.Rooms)
            .Custom((rooms, context) =>
            {
                if (rooms == null) return;

                if (rooms.Count > 1000)
                {
                    context.AddFailure("Rooms", "Cannot exceed 1,000 rooms.");
                    return;
                }

                for (int i = 0; i < rooms.Count; i++)
                {
                    if (rooms[i].Area <= 0)
                    {
                        context.AddFailure($"Rooms[{i}].Area", "Room area must be greater than zero.");
                    }
                    if (rooms[i].Area > 10000)
                    {
                        context.AddFailure($"Rooms[{i}].Area", "Room area must not exceed 10,000 square meters.");
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
