using eProrab.Domain.Common;
using eProrab.Domain.Enums;

namespace eProrab.Domain.Entities;

/// <summary>
/// A catalog item: a construction material, tool or service that eProrab
/// lists for site foremen and clients (e.g. "Portland Cement 50kg").
/// Localized name/description live in <see cref="ItemTranslation"/>.
/// </summary>
public class Item : BaseEntity
{
    /// <summary>Internal SKU/code, language-independent, unique.</summary>
    public required string Sku { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public UnitOfMeasure Unit { get; set; }

    /// <summary>Unit price in AZN (Azerbaijani Manat).</summary>
    public decimal Price { get; set; }

    public decimal? StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ItemTranslation> Translations { get; set; } = [];
}
