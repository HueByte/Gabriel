using Gabriel.Core.Entities;

namespace Gabriel.Core.Repositories;

public interface IConversationRepository
{
    // Read paths are user-scoped — a conversation only exists for its owner.
    // This way callers can't accidentally serve someone else's data by forgetting
    // to filter; the repo refuses to return cross-tenant rows.
    Task<Conversation?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<Conversation?> GetByIdWithMessagesAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Conversation>> ListAsync(Guid userId, CancellationToken ct = default);

    // Writes don't take userId — the ownership lives on the entity itself and
    // EF tracks it. Caller is expected to have already loaded the entity through
    // a user-scoped read.
    Task AddAsync(Conversation conversation, CancellationToken ct = default);
    void AddMessage(Message message);
    void Update(Conversation conversation);
    void Remove(Conversation conversation);
}
