using eProrab.Application.Interfaces;
using eProrab.Infrastructure.Options;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace eProrab.Infrastructure.Services;

/// <summary>
/// Service to validate OAuth tokens from Google and Facebook.
/// </summary>
public class OAuthTokenValidator(
    HttpClient httpClient,
    IOptions<OAuthOptions> oauthOptions) : IOAuthTokenValidator
{
    private readonly OAuthOptions _oauthOptions = oauthOptions.Value;
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// Validate a Google ID Token (NOT access_token!) using the official
    /// Google.Apis.Auth library and extract user information.
    ///
    /// ⚠️  Frontend (Vercel) tərəfindən bu endpoint-ə göndərilən JSON body:
    ///     { "credential": "<Google ID Token>" }
    ///     və ya  { "idToken": "<Google ID Token>" }
    ///
    ///     Google Sign-In JavaScript Library ilə istifadə zamanı:
    ///       - google.accounts.id.initialize callback-də gələn `response.credential`
    ///         birbaşa bu metoda ötürülməlidir.
    ///       - Heç vaxt access_token göndərməyin — o JWT deyil, opaque tokendır.
    /// </summary>
    public async Task<(string GoogleId, string Email, string FullName)> ValidateGoogleTokenAsync(
        string idToken,
        CancellationToken ct = default)
    {
        try
        {
            // Google.Apis.Auth kitabxanası:
            //   1. Google-un açıq açarlarını (JWKS) avtomatik əldə edir və keşləyir.
            //   2. İmzanı, iat/exp, iss ("accounts.google.com") yoxlayır.
            //   3. audience olaraq Google Console-dakı Client ID ilə müqayisə edir.
            //   4. sub, email, name kimi claim-ləri birbaşa payload-dan oxuyur —
            //      JwtSecurityTokenHandler-in DefaultInboundClaimTypeMap remapping
            //      problemi tamamilə aradan qalxır.
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_oauthOptions.Google.ClientId]
            };

            GoogleJsonWebSignature.Payload payload =
                await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            // payload.Subject == "sub" claim-i (unikal Google istifadəçi ID-si)
            var googleId = payload.Subject
                ?? throw new UnauthorizedAccessException("Missing 'sub' claim in Google token.");

            var email = payload.Email
                ?? throw new UnauthorizedAccessException("Missing 'email' claim in Google token.");

            // payload.Name Google hesabındakı tam ad; yoxdursa email prefix istifadə edilir
            var fullName = payload.Name ?? email.Split('@')[0];

            return (googleId, email, fullName);
        }
        catch (InvalidJwtException ex)
        {
            // Google.Apis.Auth token etibarsız və ya müddəti bitib
            throw new UnauthorizedAccessException(
                $"Google token validation failed: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException(
                $"Google token validation failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validate a Facebook access token and extract user information.
    ///
    /// ⚠️  Frontend tərəfindən: Facebook Login SDK-dan gələn accessToken
    ///     bu metoda ötürülməlidir (FB.getLoginStatus callback-də response.authResponse.accessToken).
    /// </summary>
    public async Task<(string FacebookId, string Email, string FullName)> ValidateFacebookTokenAsync(
        string idToken,
        CancellationToken ct = default)
    {
        try
        {
            // Facebook token validation endpoint
            var facebookGraphUrl = $"https://graph.facebook.com/me?fields=id,email,name&access_token={Uri.EscapeDataString(idToken)}";
            var response = await _httpClient.GetAsync(facebookGraphUrl, ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new UnauthorizedAccessException("Invalid Facebook token.");
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (root.TryGetProperty("error", out _))
            {
                throw new UnauthorizedAccessException("Facebook token validation failed.");
            }

            var facebookId = root.GetProperty("id").GetString()
                ?? throw new UnauthorizedAccessException("Missing Facebook ID in response.");

            var email = root.TryGetProperty("email", out var emailElement)
                ? (emailElement.GetString() ?? $"{facebookId}@facebook.com")
                : $"{facebookId}@facebook.com";

            var fullName = root.TryGetProperty("name", out var nameElement)
                ? (nameElement.GetString() ?? facebookId)
                : facebookId;

            return (facebookId, email, fullName);
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException(
                $"Facebook token validation failed: {ex.Message}", ex);
        }
    }
}
