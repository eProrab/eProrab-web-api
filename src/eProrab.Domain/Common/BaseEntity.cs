namespace eProrab.Domain.Common;

/// <summary>
/// Base class for all domain entities with an integer surrogate key.
/// Provides audit fields and soft-delete support used across the domain.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Soft-delete flag. Deleted rows are excluded via a global query filter
    /// so admin "delete" actions never destroy audit history.
    /// </summary>
    public bool IsDeleted { get; set; }
}
