using eProrab.Application.DTOs.Chat;

namespace eProrab.Application.Interfaces;

public interface IChatService
{
    /// <summary>Send a message from senderId to recipientId.</summary>
    Task<DirectMessageDto> SendAsync(Guid senderId, SendMessageRequest request, CancellationToken ct = default);

    /// <summary>Get all messages in the 1-on-1 thread between the two users. Only participants can read.</summary>
    Task<IReadOnlyList<DirectMessageDto>> GetThreadAsync(Guid callerId, Guid partnerId, CancellationToken ct = default);

    /// <summary>Get a list of all unique conversations the caller has participated in.</summary>
    Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsAsync(Guid callerId, CancellationToken ct = default);

    /// <summary>Mark all messages from partnerId to callerId as read.</summary>
    Task MarkReadAsync(Guid callerId, Guid partnerId, CancellationToken ct = default);

    /// <summary>Total unread count for the caller.</summary>
    Task<int> GetUnreadCountAsync(Guid callerId, CancellationToken ct = default);
}
