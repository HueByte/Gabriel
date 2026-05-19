using System.Text;
using System.Text.Json;

namespace Gabriel.Engine.Tools.Web;

public sealed class WebSearchTool : ITool
{
    private readonly IWebSearch _search;

    public WebSearchTool(IWebSearch search)
    {
        _search = search;
    }

    public string Name => "web_search";

    public string Description =>
        "Search the open web for current, external, or third-party information. " +
        "Returns top results with title, URL, and snippet. " +
        "USE THIS for: recent events, public docs of external tools/libraries, factual lookups, " +
        "current data the model wouldn't know. " +
        "DO NOT use this for questions about Gabriel itself (architecture, agent loop, " +
        "personality system, internal APIs) — use the docs_list / docs_read tools instead. " +
        "Those return Gabriel's OFFICIAL documentation; web results are at best secondhand " +
        "and may be outdated or wrong about Gabriel-specific details.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "query": {
              "type": "string",
              "description": "Search query. Be specific — short keyword strings work better than full sentences."
            },
            "limit": {
              "type": "integer",
              "description": "Max results to return (1-10). Default 5.",
              "default": 5,
              "minimum": 1,
              "maximum": 10
            }
          },
          "required": ["query"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;

        if (!root.TryGetProperty("query", out var queryEl) || queryEl.ValueKind != JsonValueKind.String)
            return "Error: 'query' is required and must be a string.";
        var query = queryEl.GetString()!;
        if (string.IsNullOrWhiteSpace(query))
            return "Error: 'query' cannot be empty.";

        var limit = 5;
        if (root.TryGetProperty("limit", out var limitEl) && limitEl.ValueKind == JsonValueKind.Number)
        {
            limit = Math.Clamp(limitEl.GetInt32(), 1, 10);
        }

        IReadOnlyList<WebSearchResult> results;
        try
        {
            results = await _search.SearchAsync(query, limit, ct);
        }
        catch (Exception ex)
        {
            return $"Error: web search failed — {ex.Message}";
        }

        if (results.Count == 0)
            return $"No results for: {query}";

        var sb = new StringBuilder();
        sb.Append("Top ").Append(results.Count).Append(" web results for: ").AppendLine(query);
        sb.AppendLine();
        for (var i = 0; i < results.Count; i++)
        {
            var r = results[i];
            sb.Append(i + 1).Append(". ").AppendLine(r.Title);
            sb.AppendLine(r.Url);
            if (!string.IsNullOrWhiteSpace(r.Snippet)) sb.AppendLine(r.Snippet);
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }
}
