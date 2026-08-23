using eProrab.Application.DTOs.Calculations;

namespace eProrab.Application.Interfaces;

public interface ICalculationService
{
    Task<SavedCalculationDto> SaveAsync(Guid userId, SaveCalculationRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<SavedCalculationDto>> GetUserCalculationsAsync(Guid userId, CancellationToken ct = default);
    Task<SavedCalculationDto?> GetByIdAsync(Guid userId, int id, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, int id, CancellationToken ct = default);
}
