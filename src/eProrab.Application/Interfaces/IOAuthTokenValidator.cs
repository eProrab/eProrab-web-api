namespace eProrab.Application.Interfaces;

/// <summary>
/// Service to validate OAuth tokens from Google and Facebook.
/// </summary>
public interface IOAuthTokenValidator
{
    /// <summary>
    /// Validate a Google ID token and extract user information.
    /// </summary>
    /// <returns>A tuple of (GoogleId, Email, FullName)</returns>
    Task<(string GoogleId, string Email, string FullName)> ValidateGoogleTokenAsync(string idToken, CancellationToken ct = default);

    /// <summary>
    /// Validate a Facebook ID token and extract user information.
    /// </summary>
    /// <returns>A tuple of (FacebookId, Email, FullName)</returns>
    Task<(string FacebookId, string Email, string FullName)> ValidateFacebookTokenAsync(string idToken, CancellationToken ct = default);
}
