using eProrab.API.Extensions;
using eProrab.Application.Common;
using eProrab.Application.Interfaces;
using eProrab.Domain.Enums;

namespace eProrab.API.Endpoints;

/// <summary>Admin oversight of the hiring system: verify/remove worker profiles, moderate job postings.</summary>
public static class AdminHiringEndpoints
{
    public static void MapAdminHiringEndpoints(this IEndpointRouteBuilder app)
    {
        var workers = app.MapGroup("/api/admin/workers")
            .WithTags("Admin - Workers")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        workers.MapGet("/", async ([AsParameters] PaginationQuery query, WorkerType? workerType, bool? isArchitectTeamMember, IWorkerService service, CancellationToken ct) =>
            Results.Ok(await service.GetPagedForAdminAsync(query, workerType, isArchitectTeamMember, ct)));

        workers.MapPatch("/{id:int}/verify", async (int id, bool isVerified, IWorkerService service, CancellationToken ct) =>
                Results.Ok(await service.SetVerifiedAsync(id, isVerified, ct)))
            .WithSummary("Mark a worker profile as identity/skill-verified (or revoke verification).");

        workers.MapDelete("/{id:int}", async (int id, IWorkerService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });

        var jobs = app.MapGroup("/api/admin/jobs")
            .WithTags("Admin - Jobs")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        jobs.MapGet("/", async ([AsParameters] PaginationQuery query, IJobService service, CancellationToken ct) =>
                Results.Ok(await service.GetPagedForAdminAsync(query, ct)))
            .WithSummary("List every job posting regardless of status, for moderation.");
    }
}

