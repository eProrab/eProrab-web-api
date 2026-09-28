namespace eProrab.Application.DTOs.Chat;

public record DirectMessageDto(
    int Id,
    Guid SenderId,
    Guid RecipientId,
    string SenderName,
    string RecipientName,
    string SenderRole,
    string RecipientRole,
    string Text,
    bool IsRead,
    string? AttachmentsJson,
    DateTime CreatedAtUtc
);

public record SendMessageRequest(
    Guid RecipientId,
    string Text,
    string? AttachmentsJson = null
);

public record ConversationSummaryDto(
    Guid PartnerId,
    string PartnerName,
    string PartnerRole,
    string LastMessageText,
    DateTime LastMessageAt,
    int UnreadCount,
    bool LastMessageIsFromMe
);

public record MarkReadRequest(Guid PartnerId);
