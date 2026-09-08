using eProrab.Application.Common;
using eProrab.Application.DTOs.Markets;

namespace eProrab.Application.Interfaces;

public interface IMarketService
{
    Task<MarketProfileDto?> GetOwnProfileAsync(Guid userId, CancellationToken ct = default);

    Task<MarketProfileDto> CreateOwnProfileAsync(Guid userId, UpsertMarketProfileRequest request, CancellationToken ct = default);

    Task<MarketProfileDto> UpdateOwnProfileAsync(Guid userId, UpsertMarketProfileRequest request, CancellationToken ct = default);

    Task<PagedResult<MarketItemDto>> GetMyItemsAsync(Guid userId, PaginationQuery query, int? categoryId, CancellationToken ct = default);

    Task<MarketItemDto> CreateItemAsync(Guid userId, CreateMarketItemRequest request, CancellationToken ct = default);

    Task<MarketItemDto> UpdateItemAsync(Guid userId, int itemId, UpdateMarketItemRequest request, CancellationToken ct = default);

    Task DeleteItemAsync(Guid userId, int itemId, CancellationToken ct = default);
}
