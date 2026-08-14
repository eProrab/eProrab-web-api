using eProrab.Application.Interfaces;
using eProrab.Domain.Entities;
using eProrab.Infrastructure.Persistence;

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
            // Simplest version: insert new snapshot rows every sync (keeps price history).
            // Swap for a real upsert-by-name-and-source if you only want current prices, not history.
            await _context.MaterialPrices.AddRangeAsync(prices, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
