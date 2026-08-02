using eProrab.Domain.Common;
using eProrab.Domain.Enums;

namespace eProrab.Domain.Entities;

/// <summary>
/// A worker's application to a <see cref="JobPosting"/>. Once accepted, this
/// row doubles as the "hire" record (AgreedRate/StartDate get filled in) so a
/// separate Hire table isn't needed for a single-application-per-hire model.
/// </summary>
public class JobApplication : BaseEntity
{
    public int JobPostingId { get; set; }

    public JobPosting JobPosting { get; set; } = null!;

    public int WorkerProfileId { get; set; }

    public WorkerProfile WorkerProfile { get; set; } = null!;

    public string? CoverMessage { get; set; }

    public decimal? ProposedRate { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public DateTime AppliedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? RespondedAtUtc { get; set; }

    /// <summary>Rate agreed once the employer accepts this application.</summary>
    public decimal? AgreedRate { get; set; }

    public DateTime? AgreedStartDate { get; set; }
}
