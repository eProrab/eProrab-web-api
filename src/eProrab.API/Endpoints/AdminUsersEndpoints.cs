using eProrab.API.Extensions;
using eProrab.API.Filters;
using eProrab.Application.Common;
using eProrab.Application.DTOs.Users;
using eProrab.Application.Interfaces;

namespace eProrab.API.Endpoints;

/// <summary>Admin panel CRUD over user accounts: create with any role, update profile,
/// change role, activate/deactivate, reset password, delete. All Admin-only.</summary>
public static class AdminUsersEndpoints
{
    public static void MapAdminUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/users")
            .WithTags("Admin - Users")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapGet("/", async ([AsParameters] PaginationQuery query, string? role, IUserService service, CancellationToken ct) =>
            Results.Ok(await service.GetPagedAsync(query, role, ct)));

        group.MapGet("/{id:guid}", async (Guid id, IUserService service, CancellationToken ct) =>
            Results.Ok(await service.GetByIdAsync(id, ct)));

        group.MapPost("/", async (CreateUserRequest request, IUserService service, CancellationToken ct) =>
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/api/admin/users/{created.Id}", created);
            })
            .WithValidation<CreateUserRequest>();

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest request, IUserService service, CancellationToken ct) =>
                Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithValidation<UpdateUserRequest>();

        group.MapPatch("/{id:guid}/role", async (Guid id, ChangeUserRoleRequest request, IUserService service, CancellationToken ct) =>
                Results.Ok(await service.ChangeRoleAsync(id, request, ct)))
            .WithValidation<ChangeUserRoleRequest>();

        group.MapPatch("/{id:guid}/active", async (Guid id, SetUserActiveRequest request, IUserService service, CancellationToken ct) =>
            Results.Ok(await service.SetActiveAsync(id, request, ct)));

        group.MapPost("/{id:guid}/reset-password", async (Guid id, AdminResetPasswordRequest request, IUserService service, CancellationToken ct) =>
            {
                await service.ResetPasswordAsync(id, request, ct);
                return Results.NoContent();
            })
            .WithValidation<AdminResetPasswordRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, IUserService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }
}
