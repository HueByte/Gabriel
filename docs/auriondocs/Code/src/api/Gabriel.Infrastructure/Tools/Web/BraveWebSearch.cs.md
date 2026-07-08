# BraveWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class BraveWebSearch : IWebSearch
```


BraveWebSearch is a concrete implementation of IWebSearch that queries the Brave Search API via a dedicated HttpClient. It validates configuration, constructs a search URL with a bounded count, executes the request, and maps Brave's response into a uniform list of WebSearchResult for consumers.

## Remarks
BraveWebSearch encapsulates provider-specific HTTP calls and Brave’s response shape behind IWebSearch, allowing callers to remain agnostic about which search provider is used. It wires HttpClientFactory, BraveSearchOptions, and ILogger to centralize concerns like authentication, timeouts, and error reporting. Only the fields we care about (title, url, description) are projected into WebSearchResult, ensuring downstream code works with a stable, provider-agnostic model.

## Notes
- The Brave API key must be configured; otherwise, BraveWebSearch throws InvalidOperationException with guidance to set Tools:Web:Brave:ApiKey.
- The requested result count is clamped to 1–10, ensuring a bounded response regardless of the caller's input.
- Non-success HTTP responses are logged with a warning and the method rethrows as HttpRequestException containing the status code; callers should handle this as a transient or propagating error as appropriate.