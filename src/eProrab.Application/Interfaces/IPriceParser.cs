using eProrab.Application.DTOs;
using eProrab.Domain.Enums;

namespace eProrab.Application.Interfaces
{
    public interface IPriceParser
    {
        PriceSource Source { get; }
        Task<List<MaterialPriceDto>> ParseAsync(CancellationToken cancellationToken = default);
    }
}