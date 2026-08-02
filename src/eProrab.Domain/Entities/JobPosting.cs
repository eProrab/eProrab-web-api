using eProrab.Domain.Common;
using eProrab.Domain.Enums;

namespace eProrab.Domain.Entities;

/// <summary>
/// A job/vacancy posted by an employer (Client, Manager or Admin) looking to
/// hire a worker for a specific <see cref="Specialization"/>. Title/description
/// are free text in whichever language the poster used (tagged by <see cref="Language"/>),
/// consistent with how user-generated content is handled across the platform.
/// </summary>
public class JobPosting : BaseEntity
{
    /// <summary>FK to the Identity user who posted the job (Infrastructure layer).</summary>
    public Guid PostedByUserId { get; set; }

    public int SpecializationId { get; set; }

    public Specialization Specialization { get; set; } = null!;

    public required string Title { get; set; }

    public required string Description { get; set; }

    public Language Language { get; set; }

    public string? City { get; set; }

    public decimal? BudgetMin { get; set; }

    public decimal? BudgetMax { get; set; }

    public BudgetType BudgetType { get; set; }

    public DateTime? StartDate { get; set; }

    public int? DurationDays { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Open;

    public ICollection<JobApplication> Applications { get; set; } = [];
}
