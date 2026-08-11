using Gabriel.Core.Entities;

namespace Gabriel.Core.Memory;

// Semantic (vector) index over saved memories. SQLite (MemoryEntry) stays the
// single source of truth; this index is derived data keyed by MemoryEntry.Id
// and can be rebuilt at any time from the relational rows. Losing or lagging
// the index degrades recall quality - it never loses user data.
//
// Contract: implementations MUST NOT throw for infrastructure failures
// (index down, embedding API unavailable). They log and degrade - Upsert/
// Delete become no-ops, Search returns empty - so a memory save never fails
// because the vector store hiccuped. Cross-user / cross-project isolation is
// enforced at the query level: every search carries a mandatory userId filter
// and a projectId filter (user-scope entries + the given project only).
public interface ISemanticMemoryIndex
{
    // False when the index is not configured (disabled in config or missing
    // dependencies). Callers can use this to fall back to non-semantic paths
    // without paying a network timeout to find out.
    bool IsEnabled { get; }

    Task UpsertAsync(MemoryEntry entry, CancellationToken ct = default);

    Task DeleteAsync(Guid memoryId, CancellationToken ct = default);

    // Semantic top-K over the caller's memories: user-scope entries plus the
    // given project's entries (projectId=null searches user-scope only).
    // Results are ordered by score descending; entries below minScore are
    // dropped by the index, not the caller.
    Task<IReadOnlyList<SemanticMemoryHit>> SearchAsync(
        string query,
        Guid userId,
        Guid? projectId,
        int topK,
        float minScore,
        CancellationToken ct = default);

    // Bulk (re)index - used by the startup backfill to converge the index with
    // SQLite. Returns the number of entries actually written.
    Task<int> ReindexAsync(IReadOnlyList<MemoryEntry> entries, CancellationToken ct = default);
}

// A search hit pointing back at the SQLite row. Body/description are not
// carried here on purpose - callers re-read the authoritative entity by Id so
// stale index payloads can never surface outdated text.
public sealed record SemanticMemoryHit(Guid MemoryId, float Score);
