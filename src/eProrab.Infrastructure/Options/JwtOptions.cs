namespace eProrab.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; set; }

    public required string Audience { get; set; }

    /// <summary>Symmetric signing key. Must be at least 32 characters in production; set via user-secrets/env var, never committed.</summary>
    public required string SigningKey { get; set; }

    public int AccessTokenMinutes { get; set; } = 30;

    public int RefreshTokenDays { get; set; } = 30;
}
