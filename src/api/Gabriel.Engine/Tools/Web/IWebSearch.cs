namespace Gabriel.Engine.Tools.Web;

// Abstraction over a web search provider (Brave, Tavily, SerpAPI, etc.).
// WebSearchTool depends on this - swap the implementation in Infrastructure
// without touching the agent layer.
public interface IWebSearch
{
    Task<IReadOnlyList<WebSearchResult>> SearchAsync(string query, int limit, CancellationToken ct);
}

public sealed record WebSearchResult(string Title, string Url, string Snippet);
