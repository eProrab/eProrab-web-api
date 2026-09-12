using eProrab.API.Extensions;
using eProrab.API.Filters;
using eProrab.Application.Common;
using eProrab.Application.DTOs.Markets;
using eProrab.Application.Interfaces;

namespace eProrab.API.Endpoints;

/// <summary>
/// The Market Cabinet endpoints: for building material stores to manage their vendor profile,
/// list repair materials with size, price, image, category and stock.
/// </summary>
public static class MarketCabinetEndpoints
{
    public static void MapMarketCabinetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/market")
            .WithTags("Market Cabinet")
            .RequireAuthorization(AuthorizationPolicies.MarketOnly);

        // Store profile endpoints
        group.MapGet("/profile", async (ICurrentUserService currentUser, IMarketService service, CancellationToken ct) =>
        {
            var profile = await service.GetOwnProfileAsync(currentUser.UserId!.Value, ct);
            return profile is null
                ? Results.NotFound(new { message = "No market profile yet — create one with POST /api/market/profile." })
                : Results.Ok(profile);
        }).WithSummary("Get caller's own store profile.");

        group.MapPost("/profile", async (UpsertMarketProfileRequest request, ICurrentUserService currentUser, IMarketService service, CancellationToken ct) =>
        {
            var created = await service.CreateOwnProfileAsync(currentUser.UserId!.Value, request, ct);
            return Results.Created("/api/market/profile", created);
        })
            .WithValidation<UpsertMarketProfileRequest>()
            .WithSummary("Create caller's store profile.");

        group.MapPut("/profile", async (UpsertMarketProfileRequest request, ICurrentUserService currentUser, IMarketService service, CancellationToken ct) =>
        {
            var updated = await service.UpdateOwnProfileAsync(currentUser.UserId!.Value, request, ct);
            return Results.Ok(updated);
        })
            .WithValidation<UpsertMarketProfileRequest>()
            .WithSummary("Update caller's store profile.");

        // Materials / Items management endpoints
        group.MapGet("/items", async ([AsParameters] PaginationQuery query, int? categoryId, ICurrentUserService currentUser, IMarketService service, CancellationToken ct) =>
        {
            var items = await service.GetMyItemsAsync(currentUser.UserId!.Value, query, categoryId, ct);
            return Results.Ok(items);
        }).WithSummary("Browse caller's own material inventory.");

        group.MapPost("/items", async (CreateMarketItemRequest request, ICurrentUserService currentUser, IMarketService service, CancellationToken ct) =>
        {
            var created = await service.CreateItemAsync(currentUser.UserId!.Value, request, ct);
            return Results.Created($"/api/market/items/{created.Id}", created);
        })
            .WithValidation<CreateMarketItemRequest>()
            .WithSummary("Add a new material to the store inventory.");

        group.MapPut("/items/{id:int}", async (int id, UpdateMarketItemRequest request, ICurrentUserService currentUser, IMarketService service, CancellationToken ct) =>
        {
            var updated = await service.UpdateItemAsync(currentUser.UserId!.Value, id, request, ct);
            return Results.Ok(updated);
        })
            .WithValidation<UpdateMarketItemRequest>()
            .WithSummary("Update a material in the store inventory.");

        group.MapDelete("/items/{id:int}", async (int id, ICurrentUserService currentUser, IMarketService service, CancellationToken ct) =>
        {
            await service.DeleteItemAsync(currentUser.UserId!.Value, id, ct);
            return Results.NoContent();
        }).WithSummary("Remove a material from the store inventory.");
    }
}
