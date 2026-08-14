using eProrab.Application.Interfaces;
using eProrab.Domain.Entities;
using eProrab.Domain.Enums;
using MediatR;

namespace eProrab.Application.Features.MaterialPrices.Commands
{
    public record SyncOmidPricesCommand : IRequest<int>;

    public class SyncOmidPricesCommandHandler : IRequestHandler<SyncOmidPricesCommand, int>
    {
        private readonly IEnumerable<IPriceParser> _parsers;
        private readonly IMaterialPriceRepository _repository;

        public SyncOmidPricesCommandHandler(IEnumerable<IPriceParser> parsers, IMaterialPriceRepository repository)
        {
            _parsers = parsers;
            _repository = repository;
        }

        public async Task<int> Handle(SyncOmidPricesCommand request, CancellationToken cancellationToken)
        {
            var parser = _parsers.First(p => p.Source == PriceSource.OmidAz);
            var scraped = await parser.ParseAsync(cancellationToken);

            var entities = scraped.Select(dto => new MaterialPrice
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Unit = dto.Unit,
                Price = dto.Price,
                Currency = dto.Currency,
                SourceUrl = dto.SourceUrl,
                Source = PriceSource.OmidAz,
                ScrapedAt = DateTime.UtcNow
            });

            await _repository.UpsertRangeAsync(entities, cancellationToken);
            return scraped.Count;
        }
    }
}
