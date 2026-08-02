using eProrab.Domain.Enums;

namespace eProrab.Domain.Entities;

/// <summary>One localized name for a <see cref="Specialization"/>.</summary>
public class SpecializationTranslation
{
    public int Id { get; set; }

    public int SpecializationId { get; set; }

    public Specialization Specialization { get; set; } = null!;

    public Language Language { get; set; }

    public required string Name { get; set; }
}
