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

    /// <summary>
    /// True for finish / overlay materials (paint, wallpaper, tiles, skirting …).
    /// False (default) for rough / structural materials (cement, brick, sand …).
    /// </summary>
    public bool IsFinishMaterial { get; set; } = false;

    /// <summary>
    /// Which building surface this finish material targets.
    /// Set by the market partner when uploading. Used by the renovation
    /// calculator to display items in the correct category tab.
    /// </summary>
    public SurfaceType SurfaceType { get; set; } = SurfaceType.None;

    /// <summary>
    /// Size specifications (e.g., "60x120 sm", "2.5x1.2 m, 12.5 mm", "50 kq kisə", "100x200x50 mm").
    /// </summary>
    public string? Dimensions { get; set; }

    /// <summary>Optional FK to the Market vendor user who listed this item.</summary>
    public Guid? MarketUserId { get; set; }

    /// <summary>Display name of the selling market / store.</summary>
    public string? MarketName { get; set; }

    public ICollection<ItemTranslation> Translations { get; set; } = [];
}

