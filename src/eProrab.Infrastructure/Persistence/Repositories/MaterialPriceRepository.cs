using eProrab.Application.Interfaces;
using eProrab.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Infrastructure.Persistence.Repositories
{
    public class MaterialPriceRepository : IMaterialPriceRepository
    {
        private readonly ApplicationDbContext _context;

        public MaterialPriceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UpsertRangeAsync(IEnumerable<MaterialPrice> prices, CancellationToken cancellationToken = default)
        {
            await _context.MaterialPrices.AddRangeAsync(prices, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<(List<MaterialPrice> Items, int TotalCount)> GetLatestAsync(
            string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            // Subquery: the latest ScrapedAt timestamp per (Name, Source)
            var latestPerGroup = _context.MaterialPrices
                .GroupBy(mp => new { mp.Name, mp.Source })
                .Select(g => new { g.Key.Name, g.Key.Source, MaxScrapedAt = g.Max(x => x.ScrapedAt) });

            // Join back to the full rows matching those latest snapshots
            var latestQuery =
                from mp in _context.MaterialPrices
                join latest in latestPerGroup
                    on new { mp.Name, mp.Source, mp.ScrapedAt }
                    equals new { latest.Name, latest.Source, ScrapedAt = latest.MaxScrapedAt }
                select mp;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                latestQuery = latestQuery.Where(mp => mp.Name.ToLower().Contains(term));
            }

            var totalCount = await latestQuery.CountAsync(cancellationToken);

            var items = await latestQuery
                .OrderBy(mp => mp.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}