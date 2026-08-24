using eProrab.Application.DTOs.Calculations;
using eProrab.Application.Interfaces;
using eProrab.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Application.Services;

public class CalculationService(IUnitOfWork uow) : ICalculationService
{
    public async Task<SavedCalculationDto> SaveAsync(Guid userId, SaveCalculationRequest request, CancellationToken ct = default)
    {
        var title = !string.IsNullOrWhiteSpace(request.Title)
            ? request.Title
            : $"{request.PropertyType} - {request.RoomCount} otaq ({request.TotalArea} m²)";

        var calculation = new SavedCalculation
        {
            UserId = userId,
            Title = title,
            PropertyType = request.PropertyType,
            StructureAge = request.StructureAge,
            RepairStyle = request.RepairStyle,
            TariffTier = request.TariffTier,
            TotalArea = request.TotalArea,
            WallHeight = request.WallHeight > 0 ? request.WallHeight : 2.8,
            RoomCount = request.RoomCount,
            DoorCount = request.DoorCount,
            WindowCount = request.WindowCount,
            TotalBudget = request.TotalBudget,
            MaterialCost = request.MaterialCost,
            LaborCost = request.LaborCost,
            OtherCost = request.OtherCost,
            RoomsJson = request.RoomsJson ?? "[]"
        };

        await uow.SavedCalculations.AddAsync(calculation, ct);
        await uow.SaveChangesAsync(ct);

        return ToDto(calculation);
    }

    public async Task<IReadOnlyList<SavedCalculationDto>> GetUserCalculationsAsync(Guid userId, CancellationToken ct = default)
    {
        var calculations = await uow.SavedCalculations.Query()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(ct);

        return calculations.Select(ToDto).ToList();
    }

    public async Task<SavedCalculationDto?> GetByIdAsync(Guid userId, int id, CancellationToken ct = default)
    {
        var calculation = await uow.SavedCalculations.Query()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

        return calculation is null ? null : ToDto(calculation);
    }

    public async Task DeleteAsync(Guid userId, int id, CancellationToken ct = default)
    {
        var calculation = await uow.SavedCalculations.Query()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

        if (calculation is not null)
        {
            uow.SavedCalculations.Remove(calculation);
            await uow.SaveChangesAsync(ct);
        }
    }

    private static SavedCalculationDto ToDto(SavedCalculation c) => new(
        c.Id,
        c.UserId,
        c.Title,
        c.PropertyType,
        c.StructureAge,
        c.RepairStyle,
        c.TariffTier,
        c.TotalArea,
        c.WallHeight,
        c.RoomCount,
        c.DoorCount,
        c.WindowCount,
        c.TotalBudget,
        c.MaterialCost,
        c.LaborCost,
        c.OtherCost,
        c.RoomsJson,
        c.CreatedAtUtc
    );
}
