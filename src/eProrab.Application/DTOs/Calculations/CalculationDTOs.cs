namespace eProrab.Application.DTOs.Calculations;

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
    decimal TotalBudget = 0,
    decimal MaterialCost = 0,
    decimal LaborCost = 0,
    decimal OtherCost = 0,
    string? RoomsJson = null
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
