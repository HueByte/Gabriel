using Gabriel.Core.Entities;

namespace Gabriel.Core.Services;

// Service layer over IMemoryRepository. Pulls UserId from ICurrentUser so
// controllers and tools don't pass it everywhere; enforces user-scoping at
// this boundary so a tool can't accidentally cross-read another user's
// memory by manipulating its arguments.
public interface IMemoryService
{
    // All memories the calling user has in the given scope. Pass projectId=null
    // for user-scope only.
    Task<IReadOnlyList<MemoryEntry>> ListAsync(Guid? projectId, CancellationToken ct = default);

    // What the agent should see for a given conversation: user-scope memories
    // plus (if applicable) the conversation's project-scope memories. Returned
    // sorted in display order (Type, then Name).
    Task<IReadOnlyList<MemoryEntry>> ListForConversationAsync(Guid? projectId, CancellationToken ct = default);

    Task<MemoryEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // Upsert: creates a new entry if (UserId, ProjectId, Name) is free, or
    // updates the existing one in place. Returns the saved entity either way.
    // Idempotent — calling twice with the same spec is a no-op apart from
    // bumping UpdatedAt.
    Task<MemoryEntry> SaveAsync(MemoryEntrySpec spec, CancellationToken ct = default);

    // Returns false if no entry matched (vs. true on actual delete) so the
    // memory_remove tool can give the model a clear "wasn't there" response.
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
    Task<bool> RemoveByNameAsync(Guid? projectId, string name, CancellationToken ct = default);
}

public sealed record MemoryEntrySpec(
    Guid? ProjectId,
    MemoryEntryType Type,
    string Name,
    string Description,
    string Body);
