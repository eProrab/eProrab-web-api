using eProrab.Domain.Entities;

namespace eProrab.Application.Interfaces
{
    public interface IMaterialPriceRepository
    {
        Task UpsertRangeAsync(IEnumerable<MaterialPrice> prices, CancellationToken cancellationToken = default);
    }
}
