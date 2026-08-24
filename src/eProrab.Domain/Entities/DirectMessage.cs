using eProrab.Domain.Common;

namespace eProrab.Domain.Entities;

/// <summary>
/// Represents a private, strictly 1-on-1 direct message between two authenticated users.
/// </summary>
public class DirectMessage : BaseEntity
{
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string RecipientRole { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? AttachmentsJson { get; set; }
    public bool IsRead { get; set; } = false;
}
