# BraveWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs`  
> **Kind:** class

```csharp
// IWebSearch implementation backed by the Brave Search API. Plain GET on
// /search?q=... + X-Subscription-Token header for auth. The named HttpClient
// is configured in DependencyInjection.AddInfrastructure so the BaseAddress +
// timeout + key header all live in one place.
public sealed class BraveWebSearch : IWebSearch
```


Implements IWebSearch using the Brave Search HTTP API; use this when you need a DI-friendly, minimal web search backed by Brave where the named HttpClient ("BraveSearch") and API key are configured externally.

## Remarks
BraveWebSearch encapsulates the HTTP call and JSON mapping so callers receive a simple IReadOnlyList<WebSearchResult>. It expects an IHttpClientFactory-provided client named "BraveSearch" to be configured with base address, timeout and the X-Subscription-Token header. The class validates configuration via BraveSearchOptions.IsConfigured, logs non-success HTTP responses (including response body), and converts the subset of Brave's response it consumes into the project's WebSearchResult DTOs.

## Example
```csharp
// Typical usage from an async context after BraveWebSearch is registered in DI
var results = await braveWebSearch.SearchAsync("open source projects", 5, CancellationToken.None);
foreach (var r in results)
{
    Console.WriteLine($"{r.Title} - {r.Url}\n{r.Snippet}\n");
}
```

## Notes
- Throws InvalidOperationException if BraveSearchOptions.IsConfigured is false (API key missing).
- The requested limit is clamped to the range 1..10 before sending to the API.
- Non-success HTTP responses are logged (warning) and surface as HttpRequestException.
- Missing or partially populated JSON fields result in empty lists/empty strings rather than nulls (safe defaults).