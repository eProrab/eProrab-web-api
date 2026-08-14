using eProrab.Domain.Enums;

namespace eProrab.Domain.Entities
{
    public class MaterialPrice
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Unit { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "AZN";
        public string SourceUrl { get; set; } = default!;
        public PriceSource Source { get; set; }
        public DateTime ScrapedAt { get; set; }
    }
}
