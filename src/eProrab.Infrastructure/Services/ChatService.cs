using eProrab.Application.DTOs.Chat;
using eProrab.Application.Interfaces;
using eProrab.Domain.Entities;
using eProrab.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Infrastructure.Services;

public class ChatService(IUnitOfWork uow, UserManager<ApplicationUser> userManager) : IChatService
{
    public async Task<DirectMessageDto> SendAsync(Guid senderId, SendMessageRequest request, CancellationToken ct = default)
    {
        var recipient = await userManager.FindByIdAsync(request.RecipientId.ToString())
            ?? throw new InvalidOperationException("Alıcı tapılmadı.");

        var sender = await userManager.FindByIdAsync(senderId.ToString())
            ?? throw new InvalidOperationException("Göndərən tapılmadı.");

        var senderRoles = await userManager.GetRolesAsync(sender);
        var recipientRoles = await userManager.GetRolesAsync(recipient);

        var message = new DirectMessage
        {
            SenderId = senderId,
            RecipientId = request.RecipientId,
            SenderName = sender.FullName,
            RecipientName = recipient.FullName,
            SenderRole = senderRoles.FirstOrDefault() ?? "Client",
            RecipientRole = recipientRoles.FirstOrDefault() ?? "Client",
            Text = request.Text,
            AttachmentsJson = request.AttachmentsJson,
            IsRead = false,
        };

        await uow.DirectMessages.AddAsync(message, ct);
        await uow.SaveChangesAsync(ct);

        return ToDto(message);
    }

    public async Task<IReadOnlyList<DirectMessageDto>> GetThreadAsync(Guid callerId, Guid partnerId, CancellationToken ct = default)
    {
        // Security: ONLY return messages where caller is sender OR recipient (Strict 1-on-1 privacy)
        var messages = await uow.DirectMessages.Query()
            .Where(m =>
                (m.SenderId == callerId && m.RecipientId == partnerId) ||
                (m.SenderId == partnerId && m.RecipientId == callerId))
            .OrderBy(m => m.CreatedAtUtc)
            .ToListAsync(ct);

        return messages.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsAsync(Guid callerId, CancellationToken ct = default)
    {
        var allMessages = await uow.DirectMessages.Query()
            .Where(m => m.SenderId == callerId || m.RecipientId == callerId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .ToListAsync(ct);

        // Group by partner
        var partnerIds = allMessages
            .Select(m => m.SenderId == callerId ? m.RecipientId : m.SenderId)
            .Distinct()
            .ToList();

        var result = new List<ConversationSummaryDto>();
        foreach (var partnerId in partnerIds)
        {
            var thread = allMessages
                .Where(m =>
                    (m.SenderId == callerId && m.RecipientId == partnerId) ||
                    (m.SenderId == partnerId && m.RecipientId == callerId))
                .OrderByDescending(m => m.CreatedAtUtc)
                .ToList();

            var last = thread.First();
            var unread = thread.Count(m => m.RecipientId == callerId && !m.IsRead);

            var partnerName = last.SenderId == callerId ? last.RecipientName : last.SenderName;
            var partnerRole = last.SenderId == callerId ? last.RecipientRole : last.SenderRole;

            result.Add(new ConversationSummaryDto(
                partnerId,
                partnerName,
                partnerRole,
                last.Text,
                last.CreatedAtUtc,
                unread,
                last.SenderId == callerId
            ));
        }

        return result.OrderByDescending(c => c.LastMessageAt).ToList();
    }

    public async Task MarkReadAsync(Guid callerId, Guid partnerId, CancellationToken ct = default)
    {
        var unread = await uow.DirectMessages.Query()
            .Where(m => m.SenderId == partnerId && m.RecipientId == callerId && !m.IsRead)
            .ToListAsync(ct);

        foreach (var msg in unread)
        {
            msg.IsRead = true;
        }

        if (unread.Count > 0)
            await uow.SaveChangesAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(Guid callerId, CancellationToken ct = default)
    {
        return await uow.DirectMessages.Query()
            .CountAsync(m => m.RecipientId == callerId && !m.IsRead, ct);
    }

    private static DirectMessageDto ToDto(DirectMessage m) => new(
        m.Id,
        m.SenderId,
        m.RecipientId,
        m.SenderName,
        m.RecipientName,
        m.SenderRole,
        m.RecipientRole,
        m.Text,
        m.IsRead,
        m.AttachmentsJson,
        m.CreatedAtUtc
    );
}
