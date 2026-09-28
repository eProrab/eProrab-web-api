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

        group.MapPost("/change-password", async (ChangePasswordRequest request, ICurrentUserService currentUser, IAuthService authService, CancellationToken ct) =>
            {
                await authService.ChangePasswordAsync(currentUser.UserId!.Value, request, ct);
                return Results.NoContent();
            })
            .WithValidation<ChangePasswordRequest>()
            .WithSummary("Change the authenticated caller's own password.")
            .RequireAuthorization();

        group.MapPut("/me", async (UpdateProfileRequest request, ICurrentUserService currentUser, IAuthService authService, CancellationToken ct) =>
                Results.Ok(await authService.UpdateProfileAsync(currentUser.UserId!.Value, request, ct)))
            .WithValidation<UpdateProfileRequest>()
            .WithSummary("Update the authenticated caller's own profile (fullName, phoneNumber).")
            .RequireAuthorization();

        group.MapPut("/role", async (SelectRoleRequest request, ICurrentUserService currentUser, IAuthService authService, CancellationToken ct) =>
                Results.Ok(await authService.SetUserRoleAsync(currentUser.UserId!.Value, request.Role, ct)))
            .WithValidation<SelectRoleRequest>()
            .WithSummary("Set or update the authenticated caller's own role (Client, Worker, Architect).")
            .RequireAuthorization();

        group.MapPost("/google-login", async (OAuthLoginRequest request, IAuthService authService, CancellationToken ct) =>
                Results.Ok(await authService.GoogleLoginAsync(request, ct)))
            .WithValidation<OAuthLoginRequest>()
            .WithSummary("Sign in or sign up with Google OAuth. Auto-creates account on first login.")
            .AllowAnonymous();

        group.MapPost("/facebook-login", async (OAuthLoginRequest request, IAuthService authService, CancellationToken ct) =>
                Results.Ok(await authService.FacebookLoginAsync(request, ct)))
            .WithValidation<OAuthLoginRequest>()
            .WithSummary("Sign in or sign up with Facebook OAuth. Auto-creates account on first login.")
            .AllowAnonymous();

        group.MapPost("/link-google", async (LinkOAuthProviderRequest request, ICurrentUserService currentUser, IAuthService authService, CancellationToken ct) =>
            {
                await authService.LinkGoogleAsync(currentUser.UserId!.Value, request, ct);
                return Results.NoContent();
            })
            .WithValidation<LinkOAuthProviderRequest>()
            .WithSummary("Link a Google account to the authenticated user's account.")
            .RequireAuthorization();

        group.MapPost("/link-facebook", async (LinkOAuthProviderRequest request, ICurrentUserService currentUser, IAuthService authService, CancellationToken ct) =>
            {
                await authService.LinkFacebookAsync(currentUser.UserId!.Value, request, ct);
                return Results.NoContent();
            })
            .WithValidation<LinkOAuthProviderRequest>()
            .WithSummary("Link a Facebook account to the authenticated user's account.")
            .RequireAuthorization();
    }
}
