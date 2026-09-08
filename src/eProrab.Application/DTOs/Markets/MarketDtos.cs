using eProrab.Application.Common;
using eProrab.Domain.Enums;

namespace eProrab.Application.DTOs.Markets;

public record MarketProfileDto(
    int Id,
    Guid UserId,
    string StoreName,
    string? Voen,
    string? Description,
    string? ContactPhone,
    string? ContactEmail,
    string? Address,
    string? City,
    string? LogoUrl,
    string? BannerUrl,
    bool IsVerified,
    bool IsActive,
    string? WorkingHours,
    int ItemsCount,
    DateTime CreatedAtUtc);

public record UpsertMarketProfileRequest(
    string StoreName,
    string? Voen,
    string? Description,
    string? ContactPhone,
    string? ContactEmail,
    string? Address,
    string? City,
    string? LogoUrl,
    string? BannerUrl,
    string? WorkingHours);

public record MarketItemDto(
    int Id,
    string Sku,
    int CategoryId,
    string CategoryName,
    UnitOfMeasure Unit,
    decimal Price,
    decimal? StockQuantity,
    string? ImageUrl,
    bool IsActive,
    bool IsFinishMaterial,
    string Name,
    string? Description,
    string? Dimensions,
    SurfaceType SurfaceType,
    DateTime CreatedAtUtc);

public record CreateMarketItemRequest(
    int CategoryId,
    string Name,
    string? Description,
    decimal Price,
    UnitOfMeasure Unit,
    decimal? StockQuantity,
    string? ImageUrl,
    string? Dimensions,
    SurfaceType SurfaceType = SurfaceType.None,
    bool IsFinishMaterial = false,
    string? Sku = null);

public record UpdateMarketItemRequest(
    int CategoryId,
    string Name,
    string? Description,
    decimal Price,
    UnitOfMeasure Unit,
    decimal? StockQuantity,
    string? ImageUrl,
    string? Dimensions,
    bool IsActive,
    SurfaceType SurfaceType = SurfaceType.None,
    bool IsFinishMaterial = false);
