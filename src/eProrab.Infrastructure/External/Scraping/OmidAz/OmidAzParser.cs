using AngleSharp;
using eProrab.Application.DTOs;
using eProrab.Application.Interfaces;
using eProrab.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace eProrab.Infrastructure.External.Scraping.OmidAz
{
    public class OmidAzParser : IPriceParser
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OmidAzParser> _logger;
        private const string ListingUrl = "https://omid.az/materials"; // TODO : double check etmek

        public PriceSource Source => PriceSource.OmidAz;

        public OmidAzParser(HttpClient httpClient, ILogger<OmidAzParser> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<MaterialPriceDto>> ParseAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<MaterialPriceDto>();

            var html = await _httpClient.GetStringAsync(ListingUrl, cancellationToken);

            var config = Configuration.Default;
            using var context = BrowsingContext.New(config);
            using var document = await context.OpenAsync(req => req.Content(html), cancellationToken);

            // TODO : omid.az DevTools inspect etmek
            var items = document.QuerySelectorAll(".product-card");

            foreach (var item in items)
            {
                try
                {
                    var name = item.QuerySelector(".product-title")?.TextContent.Trim();
                    var priceText = item.QuerySelector(".product-price")?.TextContent.Trim();
                    var unit = item.QuerySelector(".product-unit")?.TextContent.Trim();
                    var relativeUrl = item.QuerySelector("a")?.GetAttribute("href");

                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(priceText))
                        continue;

                    var price = ParsePrice(priceText);
                    if (price is null)
                        continue;

                    results.Add(new MaterialPriceDto(
                        Name: name,
                        Unit: unit,
                        Price: price.Value,
                        Currency: "AZN",
                        SourceUrl: BuildAbsoluteUrl(relativeUrl)
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse an item on omid.az listing page");
                }
            }

            _logger.LogInformation("OmidAz parser scraped {Count} items", results.Count);
            return results;
        }

        private static decimal? ParsePrice(string raw)
        {
            // Strip currency symbols/labels, keep digits, dot, comma
            var cleaned = new string(raw.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
            cleaned = cleaned.Replace(",", ".");

            return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
                ? value
                : null;
        }

        private static string BuildAbsoluteUrl(string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
                return ListingUrl;

            return relativeUrl.StartsWith("http")
                ? relativeUrl
                : $"https://omid.az{relativeUrl}";
        }
    }
}