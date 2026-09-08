namespace eProrab.Application.DTOs.Calculations;

// ─── Save (authenticated) ─────────────────────────────────────────────────────

/// <summary>
/// Request to save a calculation to the authenticated user's cabinet.
/// TotalBudget / MaterialCost / LaborCost / OtherCost are ignored — the backend
/// recalculates them from the formula to prevent client-side manipulation.
/// </summary>
public record SaveCalculationRequest(
    string? Title,
    string PropertyType,
    string StructureAge,
    string RepairStyle,
    string TariffTier,
    double TotalArea,
    double WallHeight = 2.8,
    int RoomCount = 0,
    int DoorCount = 0,
    int WindowCount = 0,
    bool IncludeRoughMaterials = true,
    string? RoomsJson = null,
    IReadOnlyList<RoomInputDto>? RoomsInput = null
);

public record SavedCalculationDto(
    int Id,
    Guid UserId,
    string Title,
    string PropertyType,
    string StructureAge,
    string RepairStyle,
    string TariffTier,
    double TotalArea,
    double WallHeight,
    int RoomCount,
    int DoorCount,
    int WindowCount,
    decimal TotalBudget,
    decimal MaterialCost,
    decimal LaborCost,
    decimal OtherCost,
    string RoomsJson,
    DateTime CreatedAtUtc
);

// ─── Estimate (public / no auth required) ────────────────────────────────────

/// <summary>
/// Request for an anonymous real-time estimate (mirrors frontend Calculator state).
/// </summary>
public record CalculationEstimateRequest(
    string PropertyType,
    string StructureAge,
    string RepairStyle,
    string TariffTier,
    double TotalArea,
    bool IncludeRoughMaterials = true,
    string? RoomsJson = null,
    IReadOnlyList<RoomInputDto>? Rooms = null
);

/// <summary>
/// Response returned by the estimate endpoint.
/// </summary>
public record CalculationEstimateResponse(
    decimal TotalBudget,
    decimal MaterialCost,
    decimal LaborCost,
    decimal OtherCost,
    decimal PerSqMeter
);

// ─── Shared sub-models ────────────────────────────────────────────────────────

public record RoomInputDto(
    double Area,
    IReadOnlyList<ComponentInputDto> Components
);

public record ComponentInputDto(
    bool Enabled,
    string? Tier
);

