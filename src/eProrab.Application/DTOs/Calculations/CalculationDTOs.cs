namespace eProrab.Application.DTOs.Calculations;

public record SaveCalculationRequest(
    string? Title,
    string PropertyType,
    string StructureAge,
    string RepairStyle,
    string TariffTier,
    double TotalArea,
    int RoomCount,
    int DoorCount,
    int WindowCount,
    decimal TotalBudget,
    decimal MaterialCost,
    decimal LaborCost,
    decimal OtherCost,
    string? RoomsJson
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
