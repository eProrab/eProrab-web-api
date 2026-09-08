using eProrab.Domain.Enums;

namespace eProrab.Application.DTOs.Auth;

/// <summary>Self-registration request. New accounts always start as Client;
/// use the admin "create user" endpoint to create Manager/Worker/Admin accounts directly.</summary>
public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string? PhoneNumber,
    Language PreferredLanguage,
    string? Role = null);

public record LoginRequest(string Email, string Password);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record UpdateProfileRequest(
    string FullName,
    string? PhoneNumber);

public record SelectRoleRequest(string Role);

public record RefreshRequest(string RefreshToken);

public record CurrentUserDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    Language PreferredLanguage,
    IReadOnlyList<string> Roles,
    bool IsActive,
    bool HasWorkerProfile,
    bool HasMarketProfile = false);

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    CurrentUserDto User,
    bool IsNewAccount = false);

/// <summary>
/// OAuth login request. The IdToken is obtained from the OAuth provider (Google/Facebook)
/// and sent by the frontend to the backend for validation and user authentication/creation.
/// </summary>
public record OAuthLoginRequest(
    string IdToken,
    Language PreferredLanguage = Language.Az,
    string? PhoneNumber = null);

/// <summary>
/// Link OAuth provider account to existing user account.
/// User must be authenticated; the IdToken is from the OAuth provider.
/// </summary>
public record LinkOAuthProviderRequest(
    string IdToken);
