using eProrab.API.Extensions;
using eProrab.API.Filters;
using eProrab.Application.Common;
using eProrab.Application.DTOs.Jobs;
using eProrab.Application.Interfaces;
using eProrab.Domain.Constants;

namespace eProrab.API.Endpoints;

/// <summary>
/// Job postings: public browsing for everyone, and full management (post, edit,
/// review applicants, accept/reject → hire) for employers — Client, Manager or Admin accounts.
/// </summary>
public static class JobEndpoints
{
    public static void MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/jobs").WithTags("Jobs");

        publicGroup.MapGet("/", async ([AsParameters] PaginationQuery query, int? specializationId, string? city, IJobService service, CancellationToken ct) =>
                Results.Ok(await service.BrowseOpenAsync(query, specializationId, city, ct)))
            .WithSummary("Browse open job postings, optionally filtered by specialization/city.")
            .AllowAnonymous();

        publicGroup.MapGet("/{id:int}", async (int id, IJobService service, CancellationToken ct) =>
                Results.Ok(await service.GetByIdAsync(id, ct)))
            .WithSummary("Get a single job posting by id.")
            .AllowAnonymous();

        var employerGroup = app.MapGroup("/api/jobs")
            .WithTags("Jobs (Employer)")
            .RequireAuthorization(AuthorizationPolicies.EmployerRoles);

        employerGroup.MapPost("/", async (CreateJobPostingRequest request, ICurrentUserService currentUser, IJobService service, CancellationToken ct) =>
            {
                var created = await service.CreateAsync(currentUser.UserId!.Value, request, ct);
                return Results.Created($"/api/jobs/{created.Id}", created);
            })
            .WithValidation<CreateJobPostingRequest>()
            .WithSummary("Post a new job (looking to hire a worker).");

        employerGroup.MapGet("/mine", async ([AsParameters] PaginationQuery query, ICurrentUserService currentUser, IJobService service, CancellationToken ct) =>
                Results.Ok(await service.GetOwnPostingsAsync(currentUser.UserId!.Value, query, ct)))
            .WithSummary("List the caller's own job postings, any status.");

        employerGroup.MapPut("/{id:int}", async (int id, UpdateJobPostingRequest request, ICurrentUserService currentUser, IJobService service, CancellationToken ct) =>
                Results.Ok(await service.UpdateAsync(id, currentUser.UserId!.Value, currentUser.IsInRole(Roles.Admin), request, ct)))
            .WithValidation<UpdateJobPostingRequest>()
            .WithSummary("Update a job posting you own (or any posting, if Admin).");

        employerGroup.MapDelete("/{id:int}", async (int id, ICurrentUserService currentUser, IJobService service, CancellationToken ct) =>
            {
                await service.DeleteAsync(id, currentUser.UserId!.Value, currentUser.IsInRole(Roles.Admin), ct);
                return Results.NoContent();
            })
            .WithSummary("Cancel/delete a job posting you own (or any posting, if Admin).");

        employerGroup.MapGet("/{id:int}/applicants", async (int id, ICurrentUserService currentUser, IJobService service, CancellationToken ct) =>
                Results.Ok(await service.GetApplicantsAsync(id, currentUser.UserId!.Value, currentUser.IsInRole(Roles.Admin), ct)))
            .WithSummary("List applicants for a job posting you own (or any posting, if Admin).");

        employerGroup.MapPost("/{id:int}/applicants/{applicationId:int}/accept",
                async (int id, int applicationId, AcceptJobApplicationRequest request, ICurrentUserService currentUser, IJobService service, CancellationToken ct) =>
                    Results.Ok(await service.AcceptApplicationAsync(id, applicationId, currentUser.UserId!.Value, currentUser.IsInRole(Roles.Admin), request, ct)))
            .WithValidation<AcceptJobApplicationRequest>()
            .WithSummary("Accept an applicant — hires them, closes the job, and auto-declines the other applicants.");

        employerGroup.MapPost("/{id:int}/applicants/{applicationId:int}/reject",
                async (int id, int applicationId, ICurrentUserService currentUser, IJobService service, CancellationToken ct) =>
                    Results.Ok(await service.RejectApplicationAsync(id, applicationId, currentUser.UserId!.Value, currentUser.IsInRole(Roles.Admin), ct)))
            .WithSummary("Reject a single applicant.");
    }
}
