# TavilySearchOptions

> **File:** `src/api/Gabriel.Core/Configuration/TavilySearchOptions.cs`  
> **Kind:** class

Represents configuration options for the Tavily web search integration. Bind this class from configuration (appsettings, environment variables, or user secrets) to provide the Tavily API base URL, bearer API key, search depth, and request timeout; use IsConfigured to determine whether the API key is present before attempting searches.

## Remarks
Centralizes the small set of settings the Tavily search tool needs so callers and DI/configuration plumbing have a single source of truth. The class is intentionally simple: it exposes a canonical SectionName for config binding, defaults suitable for common use, and an IsConfigured convenience property so callers can gracefully handle an unconfigured state instead of attempting requests that would fail.

## Example
```csharp
// appsettings.json
{
  "Tools": {
    "Web": {
      "Tavily": {
        "BaseUrl": "https://api.tavily.com/",
        "ApiKey": "tvly-xxxxxxxx",
        "SearchDepth": "basic",
        "TimeoutSeconds": 15
      }
    }
  }
}

// Startup / DI registration
services.Configure<TavilySearchOptions>(Configuration.GetSection(TavilySearchOptions.SectionName));

// Consumption
var options = serviceProvider.GetRequiredService<IOptions<TavilySearchOptions>>().Value;
if (!options.IsConfigured)
{
    // skip enabling the Tavily search tool or surface a friendly error
}
else
{
    // use options.BaseUrl, options.ApiKey, options.SearchDepth, options.TimeoutSeconds
}
```

## Notes
- BaseUrl must end with a trailing slash (e.g. "https://api.tavily.com/"); the code constructs endpoints by appending relative paths and relies on the trailing slash to produce correct URLs.
- An empty or whitespace ApiKey disables the integration; callers should check IsConfigured rather than assuming requests will succeed.
- SearchDepth accepts "basic" or "advanced"; basic is cheaper and faster (fewer pages crawled), advanced uses more credits and performs a deeper crawl.
- TimeoutSeconds is measured in seconds and defaults to 15; increase it if you expect slower upstream responses.