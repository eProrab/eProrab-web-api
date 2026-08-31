using eProrab.Domain.Enums;

namespace eProrab.Application.DTOs.Items;

/// <summary>One translation slice of an item, used both to read and to write.</summary>
public record ItemTranslationDto(Language Language, string Name, string? Description);

/// <summary>Public/catalog view — a single item resolved into the caller's requested language.</summary>
public record ItemDto(
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
    string? Description);

/// <summary>Admin view — includes every translation so the admin panel can edit all 3 languages at once.</summary>
public record ItemAdminDto(
    int Id,
    string Sku,
    int CategoryId,
    UnitOfMeasure Unit,
    decimal Price,
    decimal? StockQuantity,
    string? ImageUrl,
    bool IsActive,
    bool IsFinishMaterial,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<ItemTranslationDto> Translations);

public record CreateItemRequest(
    string Sku,
    int CategoryId,
    UnitOfMeasure Unit,
    decimal Price,
    decimal? StockQuantity,
    string? ImageUrl,
    bool IsActive,
    bool IsFinishMaterial,
    IReadOnlyList<ItemTranslationDto> Translations);

public record UpdateItemRequest(
    int CategoryId,
    UnitOfMeasure Unit,
    decimal Price,
    decimal? StockQuantity,
    string? ImageUrl,
    bool IsActive,
    bool IsFinishMaterial,
    IReadOnlyList<ItemTranslationDto> Translations);
