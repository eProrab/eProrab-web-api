using eProrab.Domain.Entities;

namespace eProrab.Application.Interfaces
{
    public interface IMaterialPriceRepository
    {
        Task UpsertRangeAsync(IEnumerable<MaterialPrice> prices, CancellationToken cancellationToken = default);

        Task<(List<MaterialPrice> Items, int TotalCount)> GetLatestAsync(
            string? search,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
