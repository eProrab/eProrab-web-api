using System.Text.Json;
using eProrab.Application.DTOs.Calculations;
using eProrab.Application.Interfaces;
using eProrab.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Application.Services;

public class CalculationService(IUnitOfWork uow) : ICalculationService
{
    // ──────────────────────────────────────────────────────────────────────────
    // Helper: map DTO rooms or raw RoomsJson → engine input rooms
    // ──────────────────────────────────────────────────────────────────────────
    private static IReadOnlyList<RoomInput>? MapRooms(IReadOnlyList<RoomInputDto>? dtoRooms, string? roomsJson = null)
    {
        if (dtoRooms is { Count: > 0 })
        {
            return dtoRooms
                .Select(r => new RoomInput(
                    r.Area,
                    r.Components
                        .Select(c => new ComponentInput(c.Enabled, c.Tier, c.Key))
                        .ToList(),
                    r.Height > 0 ? r.Height : 2.8))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(roomsJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(roomsJson);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<RoomInput>();
                    foreach (var roomEl in doc.RootElement.EnumerateArray())
                    {
                        var area = roomEl.TryGetProperty("area", out var aProp) ? aProp.GetDouble() : 0;
                        var height = roomEl.TryGetProperty("height", out var hProp) ? hProp.GetDouble() : 2.8;
                        var comps = new List<ComponentInput>();
                        if (roomEl.TryGetProperty("components", out var compsEl))
                        {
                            if (compsEl.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var compProp in compsEl.EnumerateObject())
                                {
                                    var enabled = compProp.Value.TryGetProperty("enabled", out var enProp) && enProp.GetBoolean();
                                    var tier = compProp.Value.TryGetProperty("tier", out var trProp) ? trProp.GetString() : null;
                                    comps.Add(new ComponentInput(enabled, tier, compProp.Name));
                                }
                            }
                            else if (compsEl.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in compsEl.EnumerateArray())
                                {
                                    var enabled = item.TryGetProperty("enabled", out var enProp) && enProp.GetBoolean();
                                    var tier = item.TryGetProperty("tier", out var trProp) ? trProp.GetString() : null;
                                    var key = item.TryGetProperty("key", out var kProp) ? kProp.GetString() : null;
                                    comps.Add(new ComponentInput(enabled, tier, key));
                                }
                            }
                        }
                        list.Add(new RoomInput(area, comps, height));
                    }
                    if (list.Count > 0) return list;
                }
            }
            catch
            {
                // Fallback gracefully to default factor
            }
        }

        return null;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Save (authenticated)
    // ──────────────────────────────────────────────────────────────────────────
    public async Task<SavedCalculationDto> SaveAsync(Guid userId, SaveCalculationRequest request, CancellationToken ct = default)
    {
        // Backend recalculates — never trusts client-provided budget values
        var engineInput = new CalculationInput(
            request.PropertyType,
            request.StructureAge,
            request.RepairStyle,
            request.TariffTier,
            request.TotalArea,
            request.IncludeRoughMaterials,
            MapRooms(request.RoomsInput, request.RoomsJson)
        );

        var result = CalculationEngine.Calculate(engineInput);

        var title = !string.IsNullOrWhiteSpace(request.Title)
            ? request.Title
            : $"{request.PropertyType} - {request.RoomCount} otaq ({request.TotalArea} m²)";

        var calculation = new SavedCalculation
        {
            UserId        = userId,
            Title         = title,
            PropertyType  = request.PropertyType,
            StructureAge  = request.StructureAge,
            RepairStyle   = request.RepairStyle,
            TariffTier    = request.TariffTier,
            TotalArea     = request.TotalArea,
            WallHeight    = request.WallHeight > 0 ? request.WallHeight : 2.8,
            RoomCount     = request.RoomCount,
            DoorCount     = request.DoorCount,
            WindowCount   = request.WindowCount,
            TotalBudget   = result.TotalBudget,   // ← engine result
            MaterialCost  = result.MaterialCost,  // ← engine result
            LaborCost     = result.LaborCost,     // ← engine result
            OtherCost     = result.OtherCost,     // ← engine result
            RoomsJson     = request.RoomsJson ?? "[]"
        };

        await uow.SavedCalculations.AddAsync(calculation, ct);
        await uow.SaveChangesAsync(ct);

        return ToDto(calculation);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // List / Get / Delete (authenticated)
    // ──────────────────────────────────────────────────────────────────────────
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

    // ──────────────────────────────────────────────────────────────────────────
    // Estimate (public, no auth required)
    // ──────────────────────────────────────────────────────────────────────────
    public Task<CalculationEstimateResponse> EstimateAsync(CalculationEstimateRequest request, CancellationToken ct = default)
    {
        var engineInput = new CalculationInput(
            request.PropertyType,
            request.StructureAge,
            request.RepairStyle,
            request.TariffTier,
            request.TotalArea,
            request.IncludeRoughMaterials,
            MapRooms(request.Rooms, request.RoomsJson)
        );

        var result = CalculationEngine.Calculate(engineInput);

        var perSqMeter = request.TotalArea > 0
            ? Math.Round(result.TotalBudget / (decimal)request.TotalArea, MidpointRounding.AwayFromZero)
            : 0m;

        var response = new CalculationEstimateResponse(
            result.TotalBudget,
            result.MaterialCost,
            result.LaborCost,
            result.OtherCost,
            perSqMeter
        );

        return Task.FromResult(response);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mapper
    // ──────────────────────────────────────────────────────────────────────────
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

