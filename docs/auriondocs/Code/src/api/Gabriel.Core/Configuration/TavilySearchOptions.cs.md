# TavilySearchOptions

> **File:** `src/api/Gabriel.Core/Configuration/TavilySearchOptions.cs`  
> **Kind:** class

Stores configuration for the Tavily search service used by the web/tools search integration. Use this type when binding the "Tools:Web:Tavily" configuration section or when registering the Tavily search tool so callers have a single place for defaults (BaseUrl, timeout, and search depth) and for checking whether the service has been configured (ApiKey).

## Remarks
This class centralizes the small set of options required to call the Tavily API: the service base URL, an API key, a search depth mode and a timeout. It provides sane defaults appropriate for most uses (public API base URL, 15s timeout, and "basic" search depth) and exposes IsConfigured so code can treat an empty or whitespace ApiKey as "disabled" rather than throwing at runtime. The SectionName constant makes it easy to locate or bind the corresponding configuration section.

## Example
```csharp
// appsettings.json
{
  "Tools": {
    "Web": {
      "Tavily": {
        "BaseUrl": "https://api.tavily.com/",
        "ApiKey": "tvly-...",
        "SearchDepth": "advanced",
        "TimeoutSeconds": 20
      }
    }
  }
}

// Program.cs (binding example)
var options = new TavilySearchOptions();
configuration.GetSection(TavilySearchOptions.SectionName).Bind(options);
if (!options.IsConfigured)
{
    // disable or skip registering the Tavily-based search tool
}
```

## Notes
- BaseUrl must include a trailing slash so that relative endpoints (e.g. "search") concatenate correctly.
- An empty or whitespace ApiKey disables the tool (the integration is expected to return an "unconfigured" error instead of throwing).
- SearchDepth accepts "basic" or "advanced"; basic is lower-cost/faster, advanced performs a deeper crawl and costs more credits.
- TimeoutSeconds is in seconds and defaults to 15.
- IsConfigured checks only that ApiKey is not null/empty/whitespace; it does not validate the key format or test connectivity.