# TavilySearchOptions

> **File:** `src/api/Gabriel.Core/Configuration/TavilySearchOptions.cs`  
> **Kind:** class

```csharp
public class TavilySearchOptions : IConfigSection<TavilySearchOptions>
```


TavilySearchOptions represents a configuration section for the Tavily web search integration. Implemented as [`IConfigSection<TavilySearchOptions>`](IConfigSection.cs.md), it exposes tunable settings such as the API base URL, the API key, the desired search depth, and the per-request timeout, plus a convenience IsConfigured flag that reports whether the API key is provided.

## Remarks
By encapsulating Tavily settings behind the [`IConfigSection<TavilySearchOptions>`](IConfigSection.cs.md), the application can bind this section from configuration sources (environment variables, secret stores, or appsettings) and inject a ready-to-use options instance wherever needed. The trailing slash requirement for BaseUrl ensures the relative "search" path resolves to the correct /search endpoint, avoiding URL-concatenation mistakes. The default "basic" value for SearchDepth keeps usage inexpensive for common scenarios, while "advanced" is available for more demanding queries.

## Notes
- BaseUrl must end with a trailing slash to ensure the /search endpoint is resolved correctly when combining with relative paths.
- ApiKey defaults to an empty string, which disables the Tavily tool; callers should check IsConfigured before attempting API calls.
- SectionName is the configuration binding key; ensure your configuration sources use the same key to bind values properly.