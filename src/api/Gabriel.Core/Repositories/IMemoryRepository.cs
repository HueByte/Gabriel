using Gabriel.Core.Entities;

namespace Gabriel.Core.Repositories;

public interface IMemoryRepository
{
    // List every entry in a single scope. Pass projectId=null for user-scope.
    Task<IReadOnlyList<MemoryEntry>> ListAsync(Guid userId, Guid? projectId, CancellationToken ct = default);

    // Convenience for the agent: every entry the agent should "see" for a
    // given conversation — user-scope memories + (if the conversation is in
    // a project) that project's scope-specific memories.
    Task<IReadOnlyList<MemoryEntry>> ListForAgentAsync(Guid userId, Guid? projectId, CancellationToken ct = default);

    Task<MemoryEntry?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);

    // Look up by (UserId, ProjectId, Name) — the slug is unique within scope,
    // so this is how the agent's memory_save tool decides between create and
    // update.
    Task<MemoryEntry?> FindByNameAsync(Guid userId, Guid? projectId, string name, CancellationToken ct = default);

    Task AddAsync(MemoryEntry entry, CancellationToken ct = default);
    void Update(MemoryEntry entry);
    void Remove(MemoryEntry entry);
}
