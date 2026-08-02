using eProrab.Application.Common;
using eProrab.Application.Interfaces;

namespace eProrab.API.Endpoints;

/// <summary>Public, unauthenticated catalog browsing: categories, items, specializations.
/// All content is returned already resolved into the caller's language (see ILanguageProvider).</summary>
public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var categories = app.MapGroup("/api/categories").WithTags("Catalog");
        categories.MapGet("/", async (ICategoryService service, CancellationToken ct) =>
                Results.Ok(await service.GetActiveAsync(ct)))
            .WithSummary("List active categories, localized (?lang=az|en|ru, defaults to Accept-Language or az).")
            .AllowAnonymous();

        var items = app.MapGroup("/api/items").WithTags("Catalog");
        items.MapGet("/", async ([AsParameters] PaginationQuery query, int? categoryId, IItemService service, CancellationToken ct) =>
                Results.Ok(await service.GetPublicPagedAsync(query, categoryId, ct)))
            .WithSummary("Browse active catalog items, paged and searchable, optionally filtered by category.")
            .AllowAnonymous();

        items.MapGet("/{id:int}", async (int id, IItemService service, CancellationToken ct) =>
                Results.Ok(await service.GetPublicByIdAsync(id, ct)))
            .WithSummary("Get a single active item by id.")
            .AllowAnonymous();

        var specializations = app.MapGroup("/api/specializations").WithTags("Catalog");
        specializations.MapGet("/", async (ISpecializationService service, CancellationToken ct) =>
                Results.Ok(await service.GetActiveAsync(ct)))
            .WithSummary("List active worker specializations/trades, localized.")
            .AllowAnonymous();
    }
}
