using System.Net.Http.Json;
using System.Text.Json.Serialization;
using eProrab.Application.DTOs;
using eProrab.Application.Interfaces;
using eProrab.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace eProrab.Infrastructure.External.Scraping.OmidAz;

public class OmidAzParser : IPriceParser
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OmidAzParser> _logger;

    public PriceSource Source => PriceSource.OmidAz;

    //collection handles from omid.az
    private static readonly string[] CollectionHandles =
    {
        "insaat-materiallari-f764",
        "alci-suvaq-ve-materiallari-90f1",
        "boya-mehsullari-c200",
        "santexnika-abeb",
        "elektrik-8873",
        "xirdavat-ve-el-aletleri-e809"
    };

    private const int PageLimit = 250; // Shopify's max per page

    public OmidAzParser(HttpClient httpClient, ILogger<OmidAzParser> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<MaterialPriceDto>> ParseAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<MaterialPriceDto>();

        foreach (var handle in CollectionHandles)
        {
            var page = 1;

            while (true)
            {
                var url = $"https://omid.az/collections/{handle}/products.json?limit={PageLimit}&page={page}";
                ShopifyProductsResponse? response;

                try
                {
                    var httpResponse = await _httpClient.GetAsync(url, cancellationToken);
                    _logger.LogInformation("Request to {Url} returned {StatusCode}", url, httpResponse.StatusCode);

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                        _logger.LogWarning("Non-success response body (first 300 chars): {Body}",
                            body.Length > 300 ? body[..300] : body);
                        break;
                    }

                    response = await httpResponse.Content.ReadFromJsonAsync<ShopifyProductsResponse>(cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch {Url}", url);
                    break;
                }

                if (response?.Products is null || response.Products.Count == 0)
                    break;

                foreach (var product in response.Products)
                {
                    foreach (var variant in product.Variants)
                    {
                        if (!decimal.TryParse(variant.Price, out var price))
                            continue;

                        results.Add(new MaterialPriceDto(
                            Name: variant.Title == "Default Title" ? product.Title : $"{product.Title} - {variant.Title}",
                            Unit: null,
                            Price: price,
                            Currency: "AZN",
                            SourceUrl: $"https://omid.az/products/{product.Handle}"
                        ));
                    }
                }

                _logger.LogInformation("Fetched page {Page} of {Handle}: {Count} products", page, handle, response.Products.Count);

                if (response.Products.Count < PageLimit)
                    break; // last page

                page++;
                await Task.Delay(500, cancellationToken); // be polite, don't hammer their server *_*
            }
        }

        _logger.LogInformation("OmidAz parser scraped {Count} total variants", results.Count);
        return results;
    }
}

// Shopify's public products.json response shape — internal to Infrastructure, never leaks to Application
internal class ShopifyProductsResponse
{
    [JsonPropertyName("products")]
    public List<ShopifyProduct> Products { get; set; } = new();
}

internal class ShopifyProduct
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("handle")]
    public string Handle { get; set; } = default!;

    [JsonPropertyName("vendor")]
    public string? Vendor { get; set; }

    [JsonPropertyName("variants")]
    public List<ShopifyVariant> Variants { get; set; } = new();
}

internal class ShopifyVariant
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("price")]
    public string Price { get; set; } = default!;

    [JsonPropertyName("available")]
    public bool Available { get; set; }

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }
}