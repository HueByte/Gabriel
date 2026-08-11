using System.Text;
using System.Text.Json;
using Gabriel.Core.Configuration;
using Gabriel.Core.Memory;
using Gabriel.Core.Services;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Tools.Memory;

// Semantic recall on demand: lets the agent pull memories relevant to what it
// is currently doing, mid-task, instead of relying only on the per-turn recall
// baked into the system prompt. Backed by the Qdrant index; when semantic
// memory is disabled this tool reports itself unavailable (memory_list still
// works - it's relational).
//
// Not parallel-safe: bodies are re-read from SQLite through the scoped
// IMemoryService, which shares the request DbContext.
public sealed class MemorySearchTool : ITool
{
    private const int MaxTopK = 20;

    private readonly ISemanticMemoryIndex _index;
    private readonly IMemoryService _memories;
    private readonly IToolExecutionContext _context;
    private readonly SemanticMemoryOptions _options;

    public MemorySearchTool(
        ISemanticMemoryIndex index,
        IMemoryService memories,
        IToolExecutionContext context,
        IOptions<SemanticMemoryOptions> options)
    {
        _index = index;
        _memories = memories;
        _context = context;
        _options = options.Value;
    }

    public string Name => "memory_search";

    public string Description =>
        "Semantic search over the user's saved memories (user-scope + this " +
        "project's). Finds entries related in meaning to the query, not just " +
        "exact keyword matches, and returns their full bodies ranked by " +
        "similarity. Use when the memory index in your context looks relevant " +
        "but you need the details, or when starting a task that prior " +
        "memories might inform.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "query": { "type": "string", "description": "what you're looking for, phrased naturally" },
            "top_k": { "type": "integer", "description": "max results (default 5, max 20)" }
          },
          "required": ["query"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        if (!_index.IsEnabled)
        {
            return "Error: semantic memory search is not enabled on this server. " +
                   "Use memory_list to enumerate saved memories instead.";
        }

        if (_context.UserId is not { } userId)
        {
            return "Error: no user context available for this conversation.";
        }

        MemorySearchArgs args;
        try
        {
            args = JsonSerializer.Deserialize<MemorySearchArgs>(argumentsJson, JsonOpts)
                ?? throw new InvalidOperationException("null args");
        }
        catch (Exception ex)
        {
            return $"Error: invalid arguments JSON — {ex.Message}";
        }

        if (string.IsNullOrWhiteSpace(args.Query))
        {
            return "Error: query must be non-empty.";
        }

        var topK = args.TopK is { } k ? Math.Clamp(k, 1, MaxTopK) : _options.RecallTopK;

        var hits = await _index.SearchAsync(
            args.Query, userId, _context.ProjectId, topK, _options.RecallMinScore, ct);
        if (hits.Count == 0)
        {
            return "No semantically similar memories found for that query.";
        }

        // Re-read bodies from SQLite (the source of truth) instead of trusting
        // index payloads; a hit whose row was deleted since indexing is
        // silently dropped.
        var sb = new StringBuilder();
        var shown = 0;
        foreach (var hit in hits)
        {
            var entry = await _memories.GetByIdAsync(hit.MemoryId, ct);
            if (entry is null) continue;

            shown++;
            var scope = entry.ProjectId is null ? "user" : "project";
            sb.Append("### [").Append(entry.Type.ToString().ToLowerInvariant())
              .Append(", ").Append(scope)
              .Append(", score ").Append(hit.Score.ToString("0.00"))
              .Append("] ").AppendLine(entry.Name);
            sb.AppendLine(entry.Description);
            sb.AppendLine();
            sb.AppendLine(entry.Body);
            sb.AppendLine();
        }

        return shown == 0
            ? "No semantically similar memories found for that query."
            : $"Found {shown} related memor{(shown == 1 ? "y" : "ies")}:\n\n{sb.ToString().TrimEnd()}";
    }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private sealed record MemorySearchArgs(
        string Query,
        [property: System.Text.Json.Serialization.JsonPropertyName("top_k")] int? TopK);
}
