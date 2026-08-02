using eProrab.Domain.Common;

namespace eProrab.Domain.Entities;

/// <summary>
/// A trade/profession a worker can register under (e.g. "Mason", "Electrician",
/// "Crane Operator"). Admin-curated, like <see cref="Category"/>, so it is
/// translated into all three languages instead of free text.
/// </summary>
public class Specialization : BaseEntity
{
    public required string Slug { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<SpecializationTranslation> Translations { get; set; } = [];

    public ICollection<WorkerProfile> WorkerProfiles { get; set; } = [];

    public ICollection<JobPosting> JobPostings { get; set; } = [];
}
