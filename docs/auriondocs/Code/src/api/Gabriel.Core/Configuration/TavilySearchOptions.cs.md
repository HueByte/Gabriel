# TavilySearchOptions

> **File:** `src/api/Gabriel.Core/Configuration/TavilySearchOptions.cs`  
> **Kind:** class

```csharp
public class TavilySearchOptions : IConfigSection<TavilySearchOptions>
```


TavilySearchOptions is a configuration container for the Tavily search integration; it stores the API base URL, API key, the selected search depth tier, and the per-request timeout. It implements [`IConfigSection<TavilySearchOptions>`](IConfigSection.cs.md), enabling binding to the "Tools:Web:Tavily" configuration section and exposing IsConfigured to indicate whether the API is enabled.

## Remarks
This abstraction decouples the Tavily API usage from its binding details, allowing the rest of the application to opt into the external service only when an API key is provided. The SectionName property centralizes the section key, promoting consistent configuration binding across the codebase. The defaults (BaseUrl ending with a slash, defaulting to the basic search depth, and a sensible 15-second timeout) make it safe to drop into hobby scenarios while remaining configurable for more demanding use cases.

## Example
```csharp
// Common usage: create options with a valid API key
var options = new TavilySearchOptions
{
    BaseUrl = "https://api.tavily.com/",
    ApiKey = "tvly-abc123",
    SearchDepth = "advanced",
    TimeoutSeconds = 20
};

// Check before use
if (options.IsConfigured)
{
    // Initialize and use the Tavily search client with these options
}
```

## Notes
- To enable the Tavily search feature, ensure ApiKey is non-empty; otherwise IsConfigured will be false and requests should not be issued.
- BaseUrl must end with a trailing slash to ensure the "search" endpoint is resolved correctly (as documented in the property comments).
- The defaults favor hobby usage; switching to "advanced" increases resource usage and may impact performance.