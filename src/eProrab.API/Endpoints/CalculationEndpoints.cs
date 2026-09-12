using eProrab.API.Filters;
using eProrab.Application.DTOs.Calculations;
using eProrab.Application.Interfaces;

namespace eProrab.API.Endpoints;

public static class CalculationEndpoints
{
    public static void MapCalculationEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Public: anonymous real-time estimate ──────────────────────────────
        app.MapPost("/api/calculations/estimate",
            async (CalculationEstimateRequest request, ICalculationService service, CancellationToken ct) =>
            {
                var result = await service.EstimateAsync(request, ct);
                return Results.Ok(result);
            })
            .WithValidation<CalculationEstimateRequest>()
            .WithTags("Calculations")
            .WithSummary("Compute a real-time repair cost estimate without saving. No authentication required.")
            .AllowAnonymous();

        // ── Authenticated: save / list / get / delete ─────────────────────────
        var group = app.MapGroup("/api/user/calculations")
            .WithTags("User Calculations")
            .RequireAuthorization();

        group.MapPost("/", async (SaveCalculationRequest request, ICurrentUserService currentUser, ICalculationService service, CancellationToken ct) =>
        {
            var result = await service.SaveAsync(currentUser.UserId!.Value, request, ct);
            return Results.Created($"/api/user/calculations/{result.Id}", result);
        })
            .WithValidation<SaveCalculationRequest>()
            .WithSummary("Save a repair calculation estimate to the authenticated user's cabinet.");

        group.MapGet("/", async (ICurrentUserService currentUser, ICalculationService service, CancellationToken ct) =>
        {
            var list = await service.GetUserCalculationsAsync(currentUser.UserId!.Value, ct);
            return Results.Ok(list);
        }).WithSummary("List all saved calculations belonging to the authenticated user.");

        group.MapGet("/{id:int}", async (int id, ICurrentUserService currentUser, ICalculationService service, CancellationToken ct) =>
        {
            var item = await service.GetByIdAsync(currentUser.UserId!.Value, id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithSummary("Get details of a specific saved calculation.");

        group.MapDelete("/{id:int}", async (int id, ICurrentUserService currentUser, ICalculationService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(currentUser.UserId!.Value, id, ct);
            return Results.NoContent();
        }).WithSummary("Delete a saved calculation from user cabinet.");
    }
}

