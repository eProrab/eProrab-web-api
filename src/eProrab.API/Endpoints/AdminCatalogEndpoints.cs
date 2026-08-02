using eProrab.API.Extensions;
using eProrab.API.Filters;
using eProrab.Application.Common;
using eProrab.Application.DTOs.Categories;
using eProrab.Application.DTOs.Items;
using eProrab.Application.DTOs.Specializations;
using eProrab.Application.Interfaces;

namespace eProrab.API.Endpoints;

/// <summary>Admin-only CRUD over the catalog: categories, items, specializations.
/// Every write requires all 3 translations, keeping the public catalog always fully trilingual.</summary>
public static class AdminCatalogEndpoints
{
    public static void MapAdminCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        MapCategoryAdmin(app);
        MapItemAdmin(app);
        MapSpecializationAdmin(app);
    }

    private static void MapCategoryAdmin(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/categories")
            .WithTags("Admin - Categories")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapGet("/", async ([AsParameters] PaginationQuery query, ICategoryService service, CancellationToken ct) =>
            Results.Ok(await service.GetPagedForAdminAsync(query, ct)));

        group.MapGet("/{id:int}", async (int id, ICategoryService service, CancellationToken ct) =>
            Results.Ok(await service.GetByIdForAdminAsync(id, ct)));

        group.MapPost("/", async (CreateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/api/admin/categories/{created.Id}", created);
            })
            .WithValidation<CreateCategoryRequest>();

        group.MapPut("/{id:int}", async (int id, UpdateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
                Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithValidation<UpdateCategoryRequest>();

        group.MapDelete("/{id:int}", async (int id, ICategoryService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }

    private static void MapItemAdmin(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/items")
            .WithTags("Admin - Items")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapGet("/", async ([AsParameters] PaginationQuery query, IItemService service, CancellationToken ct) =>
            Results.Ok(await service.GetPagedForAdminAsync(query, ct)));

        group.MapGet("/{id:int}", async (int id, IItemService service, CancellationToken ct) =>
            Results.Ok(await service.GetByIdForAdminAsync(id, ct)));

        group.MapPost("/", async (CreateItemRequest request, IItemService service, CancellationToken ct) =>
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/api/admin/items/{created.Id}", created);
            })
            .WithValidation<CreateItemRequest>();

        group.MapPut("/{id:int}", async (int id, UpdateItemRequest request, IItemService service, CancellationToken ct) =>
                Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithValidation<UpdateItemRequest>();

        group.MapDelete("/{id:int}", async (int id, IItemService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }

    private static void MapSpecializationAdmin(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/specializations")
            .WithTags("Admin - Specializations")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapGet("/", async ([AsParameters] PaginationQuery query, ISpecializationService service, CancellationToken ct) =>
            Results.Ok(await service.GetPagedForAdminAsync(query, ct)));

        group.MapGet("/{id:int}", async (int id, ISpecializationService service, CancellationToken ct) =>
            Results.Ok(await service.GetByIdForAdminAsync(id, ct)));

        group.MapPost("/", async (CreateSpecializationRequest request, ISpecializationService service, CancellationToken ct) =>
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/api/admin/specializations/{created.Id}", created);
            })
            .WithValidation<CreateSpecializationRequest>();

        group.MapPut("/{id:int}", async (int id, UpdateSpecializationRequest request, ISpecializationService service, CancellationToken ct) =>
                Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithValidation<UpdateSpecializationRequest>();

        group.MapDelete("/{id:int}", async (int id, ISpecializationService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }
}
