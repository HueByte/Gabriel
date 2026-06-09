# CompositeWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/CompositeWebSearch.cs`  
> **Kind:** class

Combines results from multiple IWebSearch providers in parallel and merges them into a single ranked result list. Use this when you want higher coverage and a cross-provider relevance signal (e.g., the same URL appearing in several providers should rank higher) instead of relying on a single backend.

## Remarks
This class fans a single query out to all configured providers concurrently, collects each provider's top-N (the same limit passed to SearchAsync), groups results by a canonicalized URL, and produces a merged ranking. The merge logic prefers URLs that appear across multiple providers, breaks ties by the best rank a provider gave the URL, takes the title from the provider that ranked it highest, and chooses the longest non-empty snippet across providers. Provider failures are isolated: a failing provider is logged and ignored so that other providers still contribute; if every provider fails the caller receives an empty result list.

## Example
```csharp
// Assuming implementations of IWebSearch: providerA, providerB
var providers = new List<IWebSearch> { providerA, providerB };
var logger = loggerFactory.CreateLogger<CompositeWebSearch>();
var composite = new CompositeWebSearch(providers, logger);

var results = await composite.SearchAsync("example query", limit: 10, CancellationToken.None);
// results is a merged, de-duplicated, and ranked list of WebSearchResult
```

## Notes
- Canonicalization is used only for grouping/matching URLs; the merged result preserves the first-seen (highest-ranked) raw URL for display.
- Each provider is asked for the same `limit` you pass in; the merger trims back to that limit after combining — expect more work upstream than the final returned size.
- Errors from individual providers are logged and do not stop merging; if all providers fail the method returns an empty list rather than throwing.