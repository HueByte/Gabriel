namespace Gabriel.Core.Configuration;

// Tavily Search API - purpose-built for LLM agents. Returns LLM-friendly
// snippets (already trimmed for context-window economy) and supports both
// "basic" and "advanced" search_depth tiers. Get a key at tavily.com; the
// free tier covers a generous monthly quota for hobby use.
public class TavilySearchOptions : IConfigSection<TavilySearchOptions>
{
    public static string SectionName => "Tools:Web:Tavily";

    // Tavily API base. Must end with a trailing slash so BaseAddress + relative
    // "search" lands at /search.
    public string BaseUrl { get; set; } = "https://api.tavily.com/";

    // Bearer token from app.tavily.com/home (starts with "tvly-"). Supply via
    // Infisical (TOOLS__WEB__TAVILY__APIKEY) or user-secrets. Empty = disabled
    // (the search tool returns an "unconfigured" error instead of crashing).
    public string ApiKey { get; set; } = string.Empty;

    // "basic" (1 credit/query, fast, fewer pages crawled) or "advanced"
    // (2 credits, deeper crawl, slower). Basic is plenty for the default
    // agent use case; advanced is worth it for harder "find me the source"
    // queries where the answer hides on page 3 of upstream results.
    public string SearchDepth { get; set; } = "basic";

    public int TimeoutSeconds { get; set; } = 15;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
