using eProrab.Domain.Common;

namespace eProrab.Domain.Entities;

/// <summary>
/// The "worker cabinet" data: a 1-to-1 extension of a Worker-role user with
/// trade info used for job matching. Free-text fields (Bio) are stored as the
/// worker wrote them — only the curated <see cref="Specialization"/> taxonomy
/// is translated.
/// </summary>
public class WorkerProfile : BaseEntity
{
    /// <summary>FK to the Identity user (Infrastructure layer); intentionally not a navigation property.</summary>
    public Guid UserId { get; set; }

    public int SpecializationId { get; set; }

    public Specialization Specialization { get; set; } = null!;

    public int ExperienceYears { get; set; }

    public string? Bio { get; set; }

    public string? City { get; set; }

    /// <summary>Daily rate in AZN the worker typically charges; shown to employers browsing.</summary>
    public decimal? DailyRate { get; set; }

    public bool IsAvailableForHire { get; set; } = true;

    /// <summary>Set by an Admin once the worker's identity/skills have been checked.</summary>
    public bool IsVerified { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = [];
}
