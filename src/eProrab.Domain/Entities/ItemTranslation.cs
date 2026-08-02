using eProrab.Domain.Enums;

namespace eProrab.Domain.Entities;

/// <summary>
/// One localized name/description for an <see cref="Item"/>. Exactly one
/// row per (ItemId, Language) pair — enforced by a unique index.
/// </summary>
public class ItemTranslation
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public Item Item { get; set; } = null!;

    public Language Language { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }
}
