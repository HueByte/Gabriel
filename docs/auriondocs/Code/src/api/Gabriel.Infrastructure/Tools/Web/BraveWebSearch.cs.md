# BraveWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class BraveWebSearch : IWebSearch
```


BraveWebSearch is a concrete implementation of IWebSearch that queries the Brave Search API over HTTP. It relies on a dependency-injected HttpClient (named BraveSearch) configured in DI to perform requests against Brave's /search endpoint, then maps the API payload into WebSearchResult items and returns them as a read-only list.

## Remarks

By encapsulating Brave Search specifics behind BraveWebSearch, callers stay decoupled from transport details and the Brave API payload shape. The class centralizes error handling and configuration concerns: it throws to signal missing API key configuration, logs and surfaces HTTP errors as exceptions, and translates the API response into the domain model. The internal mapping layer also ensures optional API fields default to safe empty strings, preserving a stable surface for consumers.

## Notes

- Requires BraveSearchOptions.IsConfigured; if not, an InvalidOperationException is thrown with guidance on configuring the API key.
- The requested result count is clamped to the range 1–10; values outside this range are sanitized.
- The HttpClient used must be registered under the BraveSearch name in DI; the client configuration provides BaseAddress, timeout, and the authentication header.
