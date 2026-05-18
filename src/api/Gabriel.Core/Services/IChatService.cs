using Gabriel.Core.Entities;

namespace Gabriel.Core.Services;

// Conversation CRUD orchestrator. The actual chat-turn loop (streaming, tool
// calls) lives in IAgentService — this stays focused on lifecycle.
public interface IChatService
{
    Task<Conversation> CreateConversationAsync(string? title, CancellationToken ct = default);
    Task<IReadOnlyList<Conversation>> ListConversationsAsync(CancellationToken ct = default);
    Task<Conversation> GetConversationAsync(Guid id, CancellationToken ct = default);
    Task<Conversation> RenameConversationAsync(Guid id, string title, CancellationToken ct = default);
    Task<Conversation> RerollAvatarAsync(Guid id, CancellationToken ct = default);
    Task DeleteConversationAsync(Guid id, CancellationToken ct = default);
}
