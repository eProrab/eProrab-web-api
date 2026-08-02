using eProrab.Application.Common;
using eProrab.Application.DTOs.Specializations;

namespace eProrab.Application.Interfaces;

public interface ISpecializationService
{
    Task<IReadOnlyList<SpecializationDto>> GetActiveAsync(CancellationToken ct = default);

    Task<PagedResult<SpecializationAdminDto>> GetPagedForAdminAsync(PaginationQuery query, CancellationToken ct = default);

    Task<SpecializationAdminDto> GetByIdForAdminAsync(int id, CancellationToken ct = default);

    Task<SpecializationAdminDto> CreateAsync(CreateSpecializationRequest request, CancellationToken ct = default);

    Task<SpecializationAdminDto> UpdateAsync(int id, UpdateSpecializationRequest request, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
