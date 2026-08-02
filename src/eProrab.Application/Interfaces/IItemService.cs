using eProrab.Application.Common;
using eProrab.Application.DTOs.Items;

namespace eProrab.Application.Interfaces;

public interface IItemService
{
    Task<PagedResult<ItemDto>> GetPublicPagedAsync(PaginationQuery query, int? categoryId, CancellationToken ct = default);

    Task<ItemDto> GetPublicByIdAsync(int id, CancellationToken ct = default);

    Task<PagedResult<ItemAdminDto>> GetPagedForAdminAsync(PaginationQuery query, CancellationToken ct = default);

    Task<ItemAdminDto> GetByIdForAdminAsync(int id, CancellationToken ct = default);

    Task<ItemAdminDto> CreateAsync(CreateItemRequest request, CancellationToken ct = default);

    Task<ItemAdminDto> UpdateAsync(int id, UpdateItemRequest request, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
