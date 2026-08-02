using eProrab.Application.Common;
using eProrab.Application.DTOs.Categories;

namespace eProrab.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetActiveAsync(CancellationToken ct = default);

    Task<PagedResult<CategoryAdminDto>> GetPagedForAdminAsync(PaginationQuery query, CancellationToken ct = default);

    Task<CategoryAdminDto> GetByIdForAdminAsync(int id, CancellationToken ct = default);

    Task<CategoryAdminDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);

    Task<CategoryAdminDto> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
