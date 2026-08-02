using eProrab.Domain.Common;

namespace eProrab.Domain.Entities;

/// <summary>
/// A catalog category (e.g. "Cement &amp; Concrete", "Hand Tools", "Electrical").
/// Display text lives in <see cref="CategoryTranslation"/> so every category
/// can be shown in Azerbaijani, English and Russian.
/// </summary>
public class Category : BaseEntity
{
    /// <summary>Stable, language-independent code used in URLs and imports (e.g. "cement-concrete").</summary>
    public required string Slug { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<CategoryTranslation> Translations { get; set; } = [];

    public ICollection<Item> Items { get; set; } = [];
}
