using Gabriel.Core.Entities;

namespace Gabriel.Core.Repositories;

public interface IAgentTaskRepository
{
    Task<IReadOnlyList<AgentTask>> ListAsync(Guid userId, Guid? projectId, CancellationToken ct = default);

    // Includes the log.
    Task<AgentTask?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);

    Task<AgentTask?> FindByConversationAsync(Guid conversationId, CancellationToken ct = default);

    Task<bool> IsConversationDrivenAsync(Guid conversationId, CancellationToken ct = default);

    // Worker-only, cross-user: every Running task with its owner (the worker
    // acts as that user), least recently advanced first - round-robin.
    Task<IReadOnlyList<(Guid TaskId, Guid UserId)>> ListRunnableAsync(CancellationToken ct = default);

    // Worker-only, cross-user. Includes the log.
    Task<AgentTask?> GetForWorkerAsync(Guid id, CancellationToken ct = default);

    // Re-reads the row so changes made by another scope (the API pausing or
    // cancelling mid-step) become visible.
    Task ReloadAsync(AgentTask task, CancellationToken ct = default);

    Task AddAsync(AgentTask task, CancellationToken ct = default);
}
