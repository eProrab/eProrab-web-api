namespace eProrab.Application.Services;

/// <summary>
/// Mirrors the repair cost calculation logic from the frontend (Calculator.tsx + calculatorData.ts).
/// All constants are taken directly from calculatorData.ts to stay in sync.
/// </summary>
public static class CalculationEngine
{
    // ─── Property base rates (from PROPERTY_TYPES in calculatorData.ts) ───
    private static readonly Dictionary<string, decimal> PropertyBaseRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["apartment"] = 500,
        ["villa"]     = 750,
        ["house"]     = 600,
        ["office"]    = 450,
    };

    // ─── Style multipliers (from STYLE_OPTIONS in calculatorData.ts) ───
    private static readonly Dictionary<string, decimal> StyleMultipliers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["minimalizm"] = 1.0m,
        ["modern"]     = 1.4m,
        ["klassik"]    = 2.2m,
        ["loft"]       = 1.7m,
    };

    // ─── Tariff data (from TARIFF_OPTIONS in calculatorData.ts) ───
    private sealed record TariffData(decimal PerSqMeter, decimal Multiplier);

    private static readonly Dictionary<string, TariffData> TariffOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ekonom"] = new(380m, 1.0m),
        ["orta"]   = new(550m, 1.5m),
        ["yuksek"] = new(850m, 2.4m),
    };

    // ─── Component tier weights (from Calculator.tsx lines 205-206) ───
    private static decimal GetTierWeight(string? tier) => tier?.ToLowerInvariant() switch
    {
        "ekonom" => 0.8m,
        "orta"   => 1.0m,
        "yuksek" => 1.5m,
        _        => 1.0m,
    };

    // ─── Component count assumed per room (7 components) ───
    private const int TotalComponentsPerRoom = 7;

    // ─── Rough materials base rate (from Calculator.tsx line 221) ───
    private const decimal RoughRateBase = 135m;

    // ─── Cost split ratios (from Calculator.tsx lines 227-229) ───
    private const decimal MaterialRatio = 0.6m;
    private const decimal LaborRatio    = 0.3m;
    private const decimal OtherRatio    = 0.1m;

    /// <summary>
    /// Calculates the total repair budget and its breakdown.
    /// Implements the exact same formula as useMemo(totalBudget) in Calculator.tsx.
    /// </summary>
    public static CalculationResult Calculate(CalculationInput input)
    {
        // Resolve lookup values (fallback to frontend defaults when not found)
        var baseRate        = PropertyBaseRates.GetValueOrDefault(input.PropertyType, 500m);
        var styleMultiplier = StyleMultipliers.GetValueOrDefault(input.RepairStyle, 1.4m);
        var tariff          = TariffOptions.GetValueOrDefault(input.TariffTier, new TariffData(550m, 1.5m));
        var tariffRate      = tariff.PerSqMeter;

        var totalArea = (decimal)input.TotalArea;

        // ── Component weight factor (from rooms with dynamic wall height calculation) ──
        // Mirror of: componentFactorSum calculation in Calculator.tsx
        decimal componentFactorSum = 0m;
        if (input.Rooms is { Count: > 0 })
        {
            foreach (var room in input.Rooms)
            {
                decimal roomComponentSum = 0m;
                var roomH = room.Height > 0 ? (decimal)room.Height : 2.8m;
                var wallHeightScale = roomH / 2.8m;

                foreach (var comp in room.Components)
                {
                    if (comp.Enabled)
                    {
                        var weight = GetTierWeight(comp.Tier);
                        if (string.Equals(comp.Key, "wall", StringComparison.OrdinalIgnoreCase))
                        {
                            weight *= wallHeightScale;
                        }
                        roomComponentSum += weight;
                    }
                }

                // roomWeight = (roomComponentSum / 7) * (room.area / totalArea)
                var roomArea = (decimal)room.Area;
                var roomWeight = (roomComponentSum / TotalComponentsPerRoom)
                                 * (totalArea > 0 ? roomArea / totalArea : 0m);
                componentFactorSum += roomWeight;
            }
        }

        var effectiveComponentFactor = componentFactorSum > 0m ? componentFactorSum : 1.0m;

        // ── Structure age factor ──────────────────────────────────────────
        // Mirror of: structureFactor in Calculator.tsx
        var structureFactor = string.Equals(input.StructureAge, "old", StringComparison.OrdinalIgnoreCase)
            ? 1.15m
            : 1.0m;

        // ── Base finishing cost ───────────────────────────────────────────
        // Mirror of: baseCost in Calculator.tsx
        var baseCost = totalArea
                       * ((baseRate + tariffRate) / 2m)
                       * styleMultiplier
                       * effectiveComponentFactor
                       * structureFactor;

        // ── Rough (çernovoy) materials cost ──────────────────────────────
        // Mirror of: roughMaterialsAverageCost in Calculator.tsx
        var roughRatePerSqM      = RoughRateBase * tariff.Multiplier;
        var roughMaterialsCost   = input.IncludeRoughMaterials
            ? totalArea * roughRatePerSqM
            : 0m;

        var totalBudget  = Math.Round(baseCost + roughMaterialsCost, MidpointRounding.AwayFromZero);
        var materialCost = Math.Round(totalBudget * MaterialRatio, MidpointRounding.AwayFromZero);
        var laborCost    = Math.Round(totalBudget * LaborRatio,    MidpointRounding.AwayFromZero);
        var otherCost    = Math.Round(totalBudget * OtherRatio,    MidpointRounding.AwayFromZero);

        return new CalculationResult(totalBudget, materialCost, laborCost, otherCost);
    }
}

// ─── Input / Output models ────────────────────────────────────────────────────

public sealed record CalculationInput(
    string  PropertyType,
    string  StructureAge,
    string  RepairStyle,
    string  TariffTier,
    double  TotalArea,
    bool    IncludeRoughMaterials = true,
    IReadOnlyList<RoomInput>? Rooms = null
);

public sealed record RoomInput(
    double Area,
    IReadOnlyList<ComponentInput> Components,
    double Height = 2.8
);

public sealed record ComponentInput(
    bool   Enabled,
    string? Tier,
    string? Key = null
);

public sealed record CalculationResult(
    decimal TotalBudget,
    decimal MaterialCost,
    decimal LaborCost,
    decimal OtherCost
);
