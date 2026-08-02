using eProrab.API.Extensions;
using eProrab.API.Filters;
using eProrab.Application.DTOs.Jobs;
using eProrab.Application.DTOs.Workers;
using eProrab.Application.Interfaces;

namespace eProrab.API.Endpoints;

/// <summary>
/// The "worker cabinet": everything a Worker-role account manages about themselves —
/// their trade profile, browsing/applying to jobs, and tracking their own applications.
/// </summary>
public static class WorkerCabinetEndpoints
{
    public static void MapWorkerCabinetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/worker")
            .WithTags("Worker Cabinet")
            .RequireAuthorization(AuthorizationPolicies.WorkerOnly);

        group.MapGet("/profile", async (ICurrentUserService currentUser, IWorkerService service, CancellationToken ct) =>
        {
            var profile = await service.GetOwnProfileAsync(currentUser.UserId!.Value, ct);
            return profile is null
                ? Results.NotFound(new { message = "No worker profile yet — create one with POST /api/worker/profile." })
                : Results.Ok(profile);
        }).WithSummary("Get the caller's own worker profile.");

        group.MapPost("/profile", async (UpsertWorkerProfileRequest request, ICurrentUserService currentUser, IWorkerService service, CancellationToken ct) =>
            {
                var created = await service.CreateOwnProfileAsync(currentUser.UserId!.Value, request, ct);
                return Results.Created("/api/worker/profile", created);
            })
            .WithValidation<UpsertWorkerProfileRequest>()
            .WithSummary("Create the caller's worker profile (one per account).");

        group.MapPut("/profile", async (UpsertWorkerProfileRequest request, ICurrentUserService currentUser, IWorkerService service, CancellationToken ct) =>
                Results.Ok(await service.UpdateOwnProfileAsync(currentUser.UserId!.Value, request, ct)))
            .WithValidation<UpsertWorkerProfileRequest>()
            .WithSummary("Update the caller's worker profile.");

        group.MapPost("/jobs/{jobId:int}/apply", async (int jobId, CreateJobApplicationRequest request, ICurrentUserService currentUser, IJobService service, CancellationToken ct) =>
            {
                var application = await service.ApplyAsync(jobId, currentUser.UserId!.Value, request, ct);
                return Results.Created($"/api/worker/applications/{application.Id}", application);
            })
            .WithValidation<CreateJobApplicationRequest>()
            .WithSummary("Apply to an open job posting.");

        group.MapGet("/applications", async (ICurrentUserService currentUser, IJobService service, CancellationToken ct) =>
                Results.Ok(await service.GetOwnApplicationsAsync(currentUser.UserId!.Value, ct)))
            .WithSummary("List the caller's own job applications and their statuses.");

        group.MapDelete("/applications/{applicationId:int}", async (int applicationId, ICurrentUserService currentUser, IJobService service, CancellationToken ct) =>
            {
                await service.WithdrawApplicationAsync(applicationId, currentUser.UserId!.Value, ct);
                return Results.NoContent();
            })
            .WithSummary("Withdraw a pending application.");
    }
}
