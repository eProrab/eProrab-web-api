namespace eProrab.Application.Interfaces;

public record AccessToken(string Token, DateTime ExpiresAtUtc);

/// <summary>
/// Issues JWT access tokens and opaque refresh tokens. Takes primitive claim
/// data rather than the Identity user type, so the Application layer never
/// needs to reference ASP.NET Core Identity.
/// </summary>
public interface ITokenService
{
    AccessToken GenerateAccessToken(Guid userId, string email, string fullName, IEnumerable<string> roles);

    string GenerateRefreshToken();
}
