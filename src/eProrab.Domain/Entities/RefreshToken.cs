namespace eProrab.Domain.Entities;

/// <summary>
/// A rotatable refresh token issued alongside a short-lived JWT access token.
/// Deliberately references the user only by <see cref="UserId"/> (no navigation
/// property to the Identity user type) so the Domain layer stays framework-free;
/// ASP.NET Core Identity types live in the Infrastructure layer.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public required string Token { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByToken { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
