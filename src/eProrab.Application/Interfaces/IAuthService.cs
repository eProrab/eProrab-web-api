using eProrab.Application.DTOs.Auth;

namespace eProrab.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);

    Task LogoutAsync(string refreshToken, CancellationToken ct = default);

    Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);

    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);

    Task<CurrentUserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);

    /// <summary>
    /// Authenticate via Google OAuth. Auto-creates account if user doesn't exist.
    /// If user exists with this Google ID, logs them in.
    /// </summary>
    Task<AuthResponse> GoogleLoginAsync(OAuthLoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// Authenticate via Facebook OAuth. Auto-creates account if user doesn't exist.
    /// If user exists with this Facebook ID, logs them in.
    /// </summary>
    Task<AuthResponse> FacebookLoginAsync(OAuthLoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// Link a Google OAuth account to the current authenticated user's account.
    /// </summary>
    Task LinkGoogleAsync(Guid userId, LinkOAuthProviderRequest request, CancellationToken ct = default);

    /// <summary>
    /// Link a Facebook OAuth account to the current authenticated user's account.
    /// </summary>
    Task LinkFacebookAsync(Guid userId, LinkOAuthProviderRequest request, CancellationToken ct = default);
}
