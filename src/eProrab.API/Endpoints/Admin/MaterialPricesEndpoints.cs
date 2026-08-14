using eProrab.Application.Features.MaterialPrices.Commands;
using MediatR;
namespace eProrab.API.Endpoints.Admin
{
    public static class MaterialPricesEndpoints
    {
        public static void MapMaterialPricesEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/admin/materials")
                .RequireAuthorization("AdminOnly") // adjust to your actual policy name
                .WithTags("Admin - Material Prices");

            group.MapPost("/sync/omid", async (IMediator mediator, CancellationToken ct) =>
            {
                var count = await mediator.Send(new SyncOmidPricesCommand(), ct);
                return Results.Ok(new { synced = count });
            });
        }
    }
}