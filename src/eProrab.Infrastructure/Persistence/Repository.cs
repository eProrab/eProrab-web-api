using eProrab.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Infrastructure.Persistence;

public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _set.FindAsync([id], ct);

    public IQueryable<T> Query() => _set.AsQueryable();

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}
