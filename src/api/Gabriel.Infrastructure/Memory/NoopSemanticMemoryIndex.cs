using Gabriel.Core.Entities;
using Gabriel.Core.Memory;

namespace Gabriel.Infrastructure.Memory;

// Registered when SemanticMemory:Enabled=false (the default). Keeps
// MemoryService's constructor satisfied with zero external dependencies;
// IsEnabled=false routes the agent onto the pre-Qdrant "inject everything"
// memory block and makes memory_search report itself unavailable.
public sealed class NoopSemanticMemoryIndex : ISemanticMemoryIndex
{
    public bool IsEnabled => false;

    public Task UpsertAsync(MemoryEntry entry, CancellationToken ct = default) => Task.CompletedTask;

    public Task DeleteAsync(Guid memoryId, CancellationToken ct = default) => Task.CompletedTask;

    public Task<IReadOnlyList<SemanticMemoryHit>> SearchAsync(
        string query, Guid userId, Guid? projectId, int topK, float minScore, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<SemanticMemoryHit>>(Array.Empty<SemanticMemoryHit>());

    public Task<int> ReindexAsync(IReadOnlyList<MemoryEntry> entries, CancellationToken ct = default)
        => Task.FromResult(0);
}
