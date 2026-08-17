using eProrab.Application.DTOs;
using eProrab.Application.Interfaces;
using MediatR;

namespace eProrab.Application.Features.MaterialPrices.Queries
{
    public record GetMaterialPricesQuery(string? Search, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<MaterialPriceListItemDto>>;

    public class GetMaterialPricesQueryHandler
        : IRequestHandler<GetMaterialPricesQuery, PagedResult<MaterialPriceListItemDto>>
    {
        private readonly IMaterialPriceRepository _repository;

        public GetMaterialPricesQueryHandler(IMaterialPriceRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<MaterialPriceListItemDto>> Handle(
            GetMaterialPricesQuery request, CancellationToken cancellationToken)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

            var (items, totalCount) = await _repository.GetLatestAsync(
                request.Search, page, pageSize, cancellationToken);

            var dtos = items.Select(p => new MaterialPriceListItemDto(
                p.Id, p.Name, p.Unit, p.Price, p.Currency, p.SourceUrl, p.Source.ToString(), p.ScrapedAt
            )).ToList();

            return new PagedResult<MaterialPriceListItemDto>(dtos, totalCount, page, pageSize);
        }
    }
}