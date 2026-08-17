using eProrab.Application.Features.MaterialPrices.Commands;
using eProrab.Application.Features.MaterialPrices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace eProrab.API.Endpoints
{
    public static class MaterialPricesEndpoints
    {
        public static void MapMaterialPricesEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/materials/prices", async (
            [FromServices] IMediator mediator,
            string? search,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
            {
                var query = new GetMaterialPricesQuery(search, page ?? 1, pageSize ?? 20);
                var result = await mediator.Send(query, ct);
                return Results.Ok(result);
            })
            .WithTags("Material Prices");

            // Admin-only — triggers a manual scrape
            var adminGroup = app.MapGroup("/api/admin/materials")
                .WithTags("Admin - Material Prices");
            // .RequireAuthorization("AdminOnly") — add back once that policy actually exists

            adminGroup.MapPost("/sync/omid", async (IMediator mediator, CancellationToken ct) =>
            {
                var count = await mediator.Send(new SyncOmidPricesCommand(), ct);
                return Results.Ok(new { synced = count });
            });
        }
    }
}
