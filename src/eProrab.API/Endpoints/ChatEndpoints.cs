using eProrab.API.Extensions;
using eProrab.Application.DTOs.Chat;
using eProrab.Application.Interfaces;

namespace eProrab.API.Endpoints;

/// <summary>
/// Strictly private 1-on-1 direct messaging between authenticated users.
/// Every endpoint enforces that the caller is a participant in the conversation —
/// no third party can ever read or list messages between two other users.
/// </summary>
public static class ChatEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat")
            .WithTags("Chat")
            .RequireAuthorization(); // any authenticated user

        // ── GET /api/chat/conversations ────────────────────────────────────────
        // Returns a list of all unique conversations the caller has participated in.
        group.MapGet("/conversations", async (ICurrentUserService cu, IChatService chat, CancellationToken ct) =>
            Results.Ok(await chat.GetConversationsAsync(cu.UserId!.Value, ct)))
            .WithSummary("List all conversations (inbox) for the authenticated caller.");

        // ── GET /api/chat/thread/{partnerId} ───────────────────────────────────
        // Returns all messages in the 1-on-1 thread between the caller and partnerId.
        // Also marks incoming messages as read.
        group.MapGet("/thread/{partnerId:guid}", async (Guid partnerId, ICurrentUserService cu, IChatService chat, CancellationToken ct) =>
        {
            var callerId = cu.UserId!.Value;
            var messages = await chat.GetThreadAsync(callerId, partnerId, ct);
            await chat.MarkReadAsync(callerId, partnerId, ct);
            return Results.Ok(messages);
        }).WithSummary("Get full message thread between caller and a partner. Marks incoming messages as read.");

        // ── POST /api/chat/send ────────────────────────────────────────────────
        // Send a message to a recipient.
        group.MapPost("/send", async (SendMessageRequest request, ICurrentUserService cu, IChatService chat, CancellationToken ct) =>
        {
            var msg = await chat.SendAsync(cu.UserId!.Value, request, ct);
            return Results.Created($"/api/chat/thread/{request.RecipientId}", msg);
        }).WithSummary("Send a direct message to another user.");

        // ── POST /api/chat/read/{partnerId} ───────────────────────────────────
        // Mark all messages from partnerId → caller as read.
        group.MapPost("/read/{partnerId:guid}", async (Guid partnerId, ICurrentUserService cu, IChatService chat, CancellationToken ct) =>
        {
            await chat.MarkReadAsync(cu.UserId!.Value, partnerId, ct);
            return Results.NoContent();
        }).WithSummary("Mark messages from a partner as read.");

        // ── GET /api/chat/unread-count ─────────────────────────────────────────
        group.MapGet("/unread-count", async (ICurrentUserService cu, IChatService chat, CancellationToken ct) =>
            Results.Ok(new { count = await chat.GetUnreadCountAsync(cu.UserId!.Value, ct) }))
            .WithSummary("Get total number of unread messages for the caller.");
    }
}
