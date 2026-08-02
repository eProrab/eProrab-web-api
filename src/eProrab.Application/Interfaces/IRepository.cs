namespace eProrab.Application.Interfaces;

/// <summary>
/// Generic persistence port (Repository pattern). Kept deliberately thin —
/// services compose <see cref="Query"/> with LINQ for filtering/paging/search,
/// so no per-entity repository interface is needed for simple cases.
/// Soft-deletable entities are expected to apply their own global query filter
/// in the EF configuration; this interface does not special-case deletion.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Composable queryable for filtering/sorting/paging/including navigation properties.</summary>
    IQueryable<T> Query();

    Task AddAsync(T entity, CancellationToken ct = default);

    void Update(T entity);

    void Remove(T entity);
}
