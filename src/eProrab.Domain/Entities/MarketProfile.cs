using eProrab.Domain.Common;

namespace eProrab.Domain.Entities;

/// <summary>
/// A market profile representing a building material vendor, retail store or wholesale base.
/// Holds vendor details, store name, address, contact and verification status.
/// </summary>
public class MarketProfile : BaseEntity
{
    public Guid UserId { get; set; }

    public required string StoreName { get; set; }

    public string? Voen { get; set; }

    public string? Description { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? LogoUrl { get; set; }

    public string? BannerUrl { get; set; }

    public bool IsVerified { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Working hours text, e.g. "09:00 - 19:00 (Hər gün)"</summary>
    public string? WorkingHours { get; set; }
}
