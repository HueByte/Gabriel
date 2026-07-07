# TavilySearchOptions

> **File:** `src/api/Gabriel.Core/Configuration/TavilySearchOptions.cs`  
> **Kind:** class

```csharp
public class TavilySearchOptions : IConfigSection<TavilySearchOptions>
```


TavilySearchOptions is a strongly-typed configuration container for the Tavily web search integration. It exposes the API base URL, API key, search depth tier, and request timeout, and is bound as the Tools:Web:Tavily configuration section via [`IConfigSection<TavilySearchOptions>`](IConfigSection.cs.md). Use it to configure how calls to Tavily are made; populate ApiKey from secrets or environment variables, set BaseUrl, select a depth tier ("basic" or "advanced"), and tune TimeoutSeconds. IsConfigured is a convenience flag indicating whether an API key has been provided, allowing callers to guard enabling the tool and avoid attempting requests when the credentials are missing.

## Remarks
By centralizing these settings, TavilySearchOptions decouples configuration from usage and improves testability. The SectionName constant anchors the config mapping and ensures consistency across the codebase. Because ApiKey controls access to the upstream service, IsConfigured doubles as a quick capability check for bootstrap paths and feature toggling.

## Example
```csharp
// Common usage: configure the options then initialize the client if configured
var options = new TavilySearchOptions
{
    BaseUrl = "https://api.tavily.com/",
    ApiKey = "tvly-abcdef", // securely sourced in real apps
    SearchDepth = "basic",
    TimeoutSeconds = 15
};

if (options.IsConfigured)
{
    // Initialize Tavily search client with these options
}
```

## Notes
- Ensure ApiKey is stored securely (e.g., secret stores, environment vars) and never checked in to source.
- BaseUrl must end with a trailing slash to ensure the relative /search path resolves correctly.
- The code does not validate allowed values for SearchDepth; prefer "basic" or "advanced" and perform validation at call sites if needed.