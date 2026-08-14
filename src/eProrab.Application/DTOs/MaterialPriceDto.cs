namespace eProrab.Application.DTOs
{
    public record MaterialPriceDto(
    string Name,
    string? Unit,
    decimal Price,
    string Currency,
    string SourceUrl
);
}