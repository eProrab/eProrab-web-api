using eProrab.Domain.Common;
using eProrab.Domain.Enums;

namespace eProrab.Domain.Entities;

/// <summary>
/// The "worker cabinet" data: a 1-to-1 extension of a Worker-role user with
/// trade info used for job matching. Supports both Individual Masters and Companies/Contractors.
/// </summary>
public class WorkerProfile : BaseEntity
{
    /// <summary>FK to the Identity user (Infrastructure layer); intentionally not a navigation property.</summary>
    public Guid UserId { get; set; }

    public WorkerType WorkerType { get; set; } = WorkerType.Individual;

    public string? CompanyName { get; set; }

    public string? Voen { get; set; }

    public int? TeamSize { get; set; }

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

    /// <summary>Indicates if the worker belongs to an Architect/Designer's trusted brigade or is an independent freelancer.</summary>
    public bool IsArchitectTeamMember { get; set; }

    public string? ArchitectName { get; set; }

    public string? ArchitectStudio { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = [];
}

