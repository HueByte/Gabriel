using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Gabriel.Core.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Memory;

// Qdrant-backed ISemanticMemoryIndex speaking the REST API (port 6333) via a
// named HttpClient. REST over the official gRPC client is deliberate: the
// surface we need is four endpoints, and HttpClient keeps the dependency
// graph flat and the whole thing testable with a fake handler.
//
// Honors the interface's non-throwing contract: every public method catches
// infrastructure failures, logs, and degrades (no-op writes, empty search).
// SQLite remains the source of truth; this index is rebuildable at any time
// via the startup backfill (SemanticMemoryBackfillService).
public sealed class QdrantMemoryIndex : ISemanticMemoryIndex
{
    public const string HttpClientName = "qdrant";

    // Payload value used for user-scope memories (ProjectId == null) so a
    // single keyword field can express the "user-scope OR this project"
    // filter with one match-any clause.
    private const string UserScopeSentinel = "__user__";

    private readonly IHttpClientFactory _httpFactory;
    private readonly IEmbeddingProvider _embeddings;
    private readonly SemanticMemoryOptions _options;
    private readonly ILogger<QdrantMemoryIndex> _logger;

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private volatile bool _collectionReady;

    public QdrantMemoryIndex(
        IHttpClientFactory httpFactory,
        IEmbeddingProvider embeddings,
        IOptions<SemanticMemoryOptions> options,
        ILogger<QdrantMemoryIndex> logger)
    {
        _httpFactory = httpFactory;
        _embeddings = embeddings;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsEnabled => true;

    // Collections are namespaced by embedding dimensionality: switching the
    // embedding provider (Mock 256 / Local 384 / OpenAI 1536) targets a fresh
    // collection instead of writing mismatched vectors into an existing one,
    // and the startup backfill repopulates the active collection - no manual
    // volume wipe needed when changing providers.
    private string CollectionName => $"{_options.Collection}_{_embeddings.Dimensions}d";

    public async Task UpsertAsync(MemoryEntry entry, CancellationToken ct = default)
    {
        try
        {
            if (!await EnsureCollectionAsync(ct)) return;

            var vector = await _embeddings.EmbedAsync(EmbeddingText(entry), ct);
            var client = _httpFactory.CreateClient(HttpClientName);
            using var response = await client.PutAsJsonAsync(
                $"collections/{CollectionName}/points?wait=true",
                new { points = new[] { ToPoint(entry, vector) } },
                ct);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Semantic index upsert OK | memory={MemoryId} name={Name}", entry.Id, entry.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Semantic index upsert failed (degrading, SQLite is authoritative) | memory={MemoryId}", entry.Id);
        }
    }

    public async Task DeleteAsync(Guid memoryId, CancellationToken ct = default)
    {
        try
        {
            if (!await EnsureCollectionAsync(ct)) return;

            var client = _httpFactory.CreateClient(HttpClientName);
            using var response = await client.PostAsJsonAsync(
                $"collections/{CollectionName}/points/delete?wait=true",
                new { points = new[] { memoryId } },
                ct);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Semantic index delete failed (stale point may linger until next reindex) | memory={MemoryId}", memoryId);
        }
    }

    public async Task<IReadOnlyList<SemanticMemoryHit>> SearchAsync(
        string query,
        Guid userId,
        Guid? projectId,
        int topK,
        float minScore,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || topK <= 0) return Array.Empty<SemanticMemoryHit>();
            if (!await EnsureCollectionAsync(ct)) return Array.Empty<SemanticMemoryHit>();

            var vector = await _embeddings.EmbedAsync(query, ct);

            // Isolation by construction: userId is a mandatory must-clause and
            // projectId is restricted to user-scope + the given project. There
            // is no code path that searches without these filters.
            var projectValues = projectId is { } pid
                ? new[] { UserScopeSentinel, pid.ToString() }
                : [UserScopeSentinel];

            var client = _httpFactory.CreateClient(HttpClientName);
            using var response = await client.PostAsJsonAsync(
                $"collections/{CollectionName}/points/search",
                new
                {
                    vector,
                    limit = topK,
                    score_threshold = minScore,
                    with_payload = false,
                    filter = new
                    {
                        must = new object[]
                        {
                            new { key = "userId", match = new { value = userId.ToString() } },
                            new { key = "projectId", match = new { any = projectValues } },
                        },
                    },
                },
                ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            var hits = new List<SemanticMemoryHit>();
            foreach (var item in doc.RootElement.GetProperty("result").EnumerateArray())
            {
                if (Guid.TryParse(item.GetProperty("id").GetString(), out var id))
                {
                    hits.Add(new SemanticMemoryHit(id, item.GetProperty("score").GetSingle()));
                }
            }
            return hits;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Semantic search failed (returning no hits) | user={UserId}", userId);
            return Array.Empty<SemanticMemoryHit>();
        }
    }

    public async Task<int> ReindexAsync(IReadOnlyList<MemoryEntry> entries, CancellationToken ct = default)
    {
        try
        {
            if (entries.Count == 0) return 0;
            if (!await EnsureCollectionAsync(ct)) return 0;

            var vectors = await _embeddings.EmbedBatchAsync(
                entries.Select(EmbeddingText).ToList(), ct);

            var points = entries
                .Select((entry, i) => ToPoint(entry, vectors[i]))
                .ToArray();

            var client = _httpFactory.CreateClient(HttpClientName);
            using var response = await client.PutAsJsonAsync(
                $"collections/{CollectionName}/points?wait=true",
                new { points },
                ct);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Semantic index reindexed {Count} memories", entries.Count);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Semantic reindex failed (index may lag SQLite until next attempt)");
            return 0;
        }
    }

    // The embedded document: name + description + body. Name/description carry
    // the strongest recall signal (they're written as retrieval keys), body
    // provides the long-tail tokens.
    private static string EmbeddingText(MemoryEntry entry)
        => $"{entry.Name}\n{entry.Description}\n{entry.Body}";

    private static object ToPoint(MemoryEntry entry, float[] vector) => new
    {
        id = entry.Id,
        vector,
        payload = new
        {
            userId = entry.UserId.ToString(),
            projectId = entry.ProjectId?.ToString() ?? UserScopeSentinel,
            name = entry.Name,
            type = entry.Type.ToString().ToLowerInvariant(),
            updatedAt = entry.UpdatedAt,
        },
    };

    // Creates the collection + payload indexes on first use. Result is cached
    // on success only, so an offline Qdrant is retried on the next call rather
    // than wedging the index disabled until restart.
    private async Task<bool> EnsureCollectionAsync(CancellationToken ct)
    {
        if (_collectionReady) return true;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_collectionReady) return true;

            var client = _httpFactory.CreateClient(HttpClientName);
            using var probe = await client.GetAsync($"collections/{CollectionName}", ct);
            if (probe.StatusCode == HttpStatusCode.NotFound)
            {
                using var create = await client.PutAsJsonAsync(
                    $"collections/{CollectionName}",
                    new { vectors = new { size = _embeddings.Dimensions, distance = "Cosine" } },
                    ct);
                create.EnsureSuccessStatusCode();

                // Keyword indexes make the mandatory userId/projectId filters
                // cheap. Failures here are non-fatal - unindexed filters are
                // slower, not wrong.
                foreach (var field in new[] { "userId", "projectId" })
                {
                    using var index = await client.PutAsJsonAsync(
                        $"collections/{CollectionName}/index",
                        new { field_name = field, field_schema = "keyword" },
                        ct);
                    if (!index.IsSuccessStatusCode)
                    {
                        _logger.LogDebug("Payload index creation for {Field} returned {Status}", field, index.StatusCode);
                    }
                }

                _logger.LogInformation(
                    "Created Qdrant collection '{Collection}' ({Dims} dims, cosine, embeddings={Provider})",
                    CollectionName, _embeddings.Dimensions, _embeddings.Name);
            }
            else
            {
                probe.EnsureSuccessStatusCode();
            }

            _collectionReady = true;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Qdrant unreachable at {Url} (semantic memory degrading to no-op this call)", _options.QdrantUrl);
            return false;
        }
        finally
        {
            _initLock.Release();
        }
    }
}
