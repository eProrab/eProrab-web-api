using eProrab.Application.Interfaces;
using eProrab.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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
    /// Validate a Google ID token and extract user information.
    /// </summary>
    public async Task<(string GoogleId, string Email, string FullName)> ValidateGoogleTokenAsync(string idToken, CancellationToken ct = default)
    {
        try
        {
            // GET Google's public keys
            const string googleJwksUrl = "https://www.googleapis.com/oauth2/v3/certs";
            var response = await _httpClient.GetAsync(googleJwksUrl, ct);
            response.EnsureSuccessStatusCode();
            var jwksContent = await response.Content.ReadAsStringAsync(ct);

            // Parse the token without validating signature first (to get kid)
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadToken(idToken) as JwtSecurityToken 
                ?? throw new UnauthorizedAccessException("Invalid token format.");

            // Decode header to get key ID
            var kid = token.Header["kid"]?.ToString();

            // Parse JWKS to find the matching key
            using var jDoc = JsonDocument.Parse(jwksContent);
            var keysElement = jDoc.RootElement.GetProperty("keys");
            JsonElement? matchingKey = null;

            foreach (var keyElement in keysElement.EnumerateArray())
            {
                if (keyElement.TryGetProperty("kid", out var keyKid) && keyKid.GetString() == kid)
                {
                    matchingKey = keyElement;
                    break;
                }
            }

            if (matchingKey == null)
            {
                throw new UnauthorizedAccessException("Could not find matching key for token.");
            }

            // Build security key from JWKS key
            var keyObj = matchingKey.Value;
            var e = keyObj.GetProperty("e").GetString();
            var n = keyObj.GetProperty("n").GetString();

            var rsa = System.Security.Cryptography.RSA.Create();
            var rsaParameters = RSAParametersExtensions.CreateRSAParameters(e, n);
            rsa.ImportParameters(rsaParameters);
            var securityKey = new RsaSecurityKey(rsa) { KeyId = kid };

            // Validate token
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://accounts.google.com",
                ValidateAudience = true,
                ValidAudience = _oauthOptions.Google.ClientId,
                ValidateLifetime = true,
                IssuerSigningKey = securityKey,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(idToken, validationParameters, out var validatedToken);

            // Extract user info from claims
            var googleId = principal.FindFirst("sub")?.Value 
                ?? throw new UnauthorizedAccessException("Missing 'sub' claim in Google token.");
            var email = principal.FindFirst("email")?.Value 
                ?? throw new UnauthorizedAccessException("Missing 'email' claim in Google token.");
            var fullName = principal.FindFirst("name")?.Value ?? email.Split('@')[0];

            return (googleId, email, fullName);
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException))
        {
            throw new UnauthorizedAccessException($"Google token validation failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validate a Facebook ID token and extract user information.
    /// </summary>
    public async Task<(string FacebookId, string Email, string FullName)> ValidateFacebookTokenAsync(string idToken, CancellationToken ct = default)
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
                ? emailElement.GetString() 
                : $"{facebookId}@facebook.com";
            var fullName = root.TryGetProperty("name", out var nameElement) 
                ? nameElement.GetString() 
                : facebookId;

            return (facebookId, email, fullName);
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException))
        {
            throw new UnauthorizedAccessException($"Facebook token validation failed: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Helper extension for RSA parameter conversion from JWKS.
/// </summary>
internal static class RSAParametersExtensions
{
    public static System.Security.Cryptography.RSAParameters CreateRSAParameters(string e, string n)
    {
        var exponent = Base64UrlDecode(e);
        var modulus = Base64UrlDecode(n);

        return new System.Security.Cryptography.RSAParameters
        {
            Exponent = exponent,
            Modulus = modulus
        };
    }

    private static byte[] Base64UrlDecode(string base64Url)
    {
        var padded = base64Url.Length % 4 == 0
            ? base64Url
            : base64Url + new string('=', 4 - base64Url.Length % 4);

        var base64 = padded
            .Replace("_", "/")
            .Replace("-", "+");

        return Convert.FromBase64String(base64);
    }
}
