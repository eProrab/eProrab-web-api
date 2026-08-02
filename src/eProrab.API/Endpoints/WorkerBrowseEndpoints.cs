using eProrab.Application.Common;
using eProrab.Application.Interfaces;

namespace eProrab.API.Endpoints;

/// <summary>Public/employer-facing browsing of available workers (headhunting, outside the job-application flow).</summary>
public static class WorkerBrowseEndpoints
{
    public static void MapWorkerBrowseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workers").WithTags("Workers (Browse)");

        group.MapGet("/", async ([AsParameters] PaginationQuery query, int? specializationId, string? city, IWorkerService service, CancellationToken ct) =>
                Results.Ok(await service.BrowseAsync(query, specializationId, city, ct)))
            .WithSummary("Browse workers available for hire, optionally filtered by specialization/city.")
            .AllowAnonymous();

        group.MapGet("/{id:int}", async (int id, IWorkerService service, CancellationToken ct) =>
                Results.Ok(await service.GetByIdAsync(id, ct)))
            .WithSummary("Get a single worker's public profile.")
            .AllowAnonymous();
    }
}
