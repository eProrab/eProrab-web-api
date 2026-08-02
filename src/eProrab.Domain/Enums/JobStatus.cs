namespace eProrab.Domain.Enums;

/// <summary>Lifecycle of a job posting created by an employer (Client/Manager/Admin).</summary>
public enum JobStatus
{
    Open = 0,
    Filled = 1,
    Closed = 2,
    Cancelled = 3
}
