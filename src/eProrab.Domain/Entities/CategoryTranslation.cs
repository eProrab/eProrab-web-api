using eProrab.Domain.Enums;

namespace eProrab.Domain.Entities;

/// <summary>
/// One localized name for a <see cref="Category"/>. Exactly one row per
/// (CategoryId, Language) pair — enforced by a unique index in the DB configuration.
/// </summary>
public class CategoryTranslation
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public Language Language { get; set; }

    public required string Name { get; set; }
}
