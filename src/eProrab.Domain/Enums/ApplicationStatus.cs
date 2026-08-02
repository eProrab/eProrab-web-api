namespace eProrab.Domain.Enums;

/// <summary>Lifecycle of a worker's application to a job posting.</summary>
public enum ApplicationStatus
{
    Pending = 0,
    Reviewed = 1,
    Accepted = 2,
    Rejected = 3,
    Withdrawn = 4
}
