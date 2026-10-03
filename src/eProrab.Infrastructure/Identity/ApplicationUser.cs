using eProrab.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace eProrab.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity user. Deliberately lives in Infrastructure (not Domain)
/// so the Domain layer stays free of framework dependencies; Application code
/// only ever sees this through <see cref="eProrab.Application.Interfaces.UserSummary"/>
/// or the auth/user DTOs.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public required string FullName { get; set; }

    public Language PreferredLanguage { get; set; } = Language.Az;

    public bool IsActive { get; set; } = true;

    public bool MustChangePassword { get; set; } = false;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // OAuth provider associations
    public string? GoogleId { get; set; }

    public string? FacebookId { get; set; }

    public DateTime? GoogleLinkedAtUtc { get; set; }

    public DateTime? FacebookLinkedAtUtc { get; set; }
}
