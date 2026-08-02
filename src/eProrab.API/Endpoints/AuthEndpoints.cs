using eProrab.API.Filters;
using eProrab.Application.DTOs.Auth;
using eProrab.Application.Interfaces;

namespace eProrab.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService authService, CancellationToken ct) =>
                Results.Ok(await authService.RegisterAsync(request, ct)))
            .WithValidation<RegisterRequest>()
            .WithSummary("Self-register a new Client account.")
            .AllowAnonymous();

        group.MapPost("/login", async (LoginRequest request, IAuthService authService, CancellationToken ct) =>
                Results.Ok(await authService.LoginAsync(request, ct)))
            .WithValidation<LoginRequest>()
            .WithSummary("Log in with email + password; returns a JWT access token and a refresh token.")
            .AllowAnonymous();

        group.MapPost("/refresh", async (RefreshRequest request, IAuthService authService, CancellationToken ct) =>
                Results.Ok(await authService.RefreshAsync(request.RefreshToken, ct)))
            .WithValidation<RefreshRequest>()
            .WithSummary("Exchange a still-valid refresh token for a new access/refresh token pair.")
            .AllowAnonymous();

        group.MapPost("/logout", async (RefreshRequest request, IAuthService authService, CancellationToken ct) =>
            {
                await authService.LogoutAsync(request.RefreshToken, ct);
                return Results.NoContent();
            })
            .WithValidation<RefreshRequest>()
            .WithSummary("Revoke a refresh token.")
            .AllowAnonymous();

        group.MapGet("/me", async (ICurrentUserService currentUser, IAuthService authService, CancellationToken ct) =>
                Results.Ok(await authService.GetCurrentUserAsync(currentUser.UserId!.Value, ct)))
            .WithSummary("Get the authenticated caller's own profile and roles.")
            .RequireAuthorization();
    }
}
