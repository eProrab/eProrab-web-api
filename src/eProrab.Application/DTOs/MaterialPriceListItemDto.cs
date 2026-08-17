namespace eProrab.Application.DTOs
{
    public record MaterialPriceListItemDto(
    Guid Id,
    string Name,
    string? Unit,
    decimal Price,
    string Currency,
    string SourceUrl,
    string Source,
    DateTime ScrapedAt
);

    public record PagedResult<T>(
        List<T> Items,
        int TotalCount,
        int Page,
        int PageSize
    );
}