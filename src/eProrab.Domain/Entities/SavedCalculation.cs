using eProrab.Domain.Common;

namespace eProrab.Domain.Entities;

public class SavedCalculation : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string StructureAge { get; set; } = string.Empty;
    public string RepairStyle { get; set; } = string.Empty;
    public string TariffTier { get; set; } = string.Empty;
    public double TotalArea { get; set; }
    public double WallHeight { get; set; } = 2.8;
    public int RoomCount { get; set; }
    public int DoorCount { get; set; }
    public int WindowCount { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal MaterialCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal OtherCost { get; set; }
    public string RoomsJson { get; set; } = "[]";
}
