using Gabriel.Core.Entities;

namespace Gabriel.Core.Services;

// Conversation CRUD orchestrator. The actual chat-turn loop (streaming, tool
// calls) lives in IAgentService — this stays focused on lifecycle.
public interface IChatService
{
    // `projectId` is optional — if absent, the conversation falls into the
    // user's Default project (auto-created on first call).
    Task<Conversation> CreateConversationAsync(Guid? projectId, string? title, CancellationToken ct = default);

    // List the user's conversations. Optionally filter by project — null means
    // "all conversations across all projects".
    Task<IReadOnlyList<Conversation>> ListConversationsAsync(Guid? projectId, CancellationToken ct = default);

    Task<Conversation> GetConversationAsync(Guid id, CancellationToken ct = default);
    Task<Conversation> RenameConversationAsync(Guid id, string title, CancellationToken ct = default);
    Task<Conversation> RerollAvatarAsync(Guid id, CancellationToken ct = default);

    // Pin the conversation's avatar pattern + palette (the "skin"). Mirror of
    // IProjectService.SetSkinAsync; only meaningful for standalone (Default-
    // project) chats — chats inside a real project render the project's
    // skin, not the conversation's. Catalog identifiers must be validated at
    // the API layer before reaching here.
    Task<Conversation> SetSkinAsync(Guid id, string? pattern, string? palette, CancellationToken ct = default);

    Task DeleteConversationAsync(Guid id, CancellationToken ct = default);

    // Message-level operations.
    //
    // Delete is destructive: it removes the targeted message AND everything that
    // came after it (anchored on the variant group's earliest sibling so a regen
    // tail is wiped cleanly). Used for "rewind this thread to here" UX.
    Task<Conversation> DeleteMessageAsync(Guid conversationId, Guid messageId, CancellationToken ct = default);

    // Switches which message variant is currently active within its variant
    // group. All siblings get flipped inactive; the chosen one becomes active.
    // No-op if the chosen message is already the active variant.
    Task<Conversation> SetActiveVariantAsync(Guid conversationId, Guid messageId, CancellationToken ct = default);
}
