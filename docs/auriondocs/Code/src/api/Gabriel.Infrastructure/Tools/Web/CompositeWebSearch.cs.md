# CompositeWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/CompositeWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class CompositeWebSearch : IWebSearch
```


CompositeWebSearch fans a single query out to several backing providers in parallel and merges their results to present a unified set of hits. It is used when you want broader coverage and cross-provider quality signals rather than relying on a single backend. The merge uses a canonical URL key to group duplicates across providers and a simple scoring rule that prioritizes URLs seen by multiple providers while respecting each provider’s ranking. The original URL from the highest-ranked provider is kept for display, while the grouping logic is purely for matching and deduplication.

## Remarks
CompositeWebSearch serves as an orchestration layer that decouples provider-specific ranking from the final merged ranking. By dispatching requests in parallel and merging results, it improves coverage and resilience: one failing provider does not invalidate the whole result set, and cross-provider hits rise in prominence. The design makes it straightforward to adjust provider selection, limit behavior, and the merge strategy without changing individual backends.

## Example
```csharp
// Example usage: combine two backends for broader coverage
IReadOnlyList<IWebSearch> providers = new IWebSearch[] { googleProvider, bingProvider };
var composite = new CompositeWebSearch(providers, logger);
var results = await composite.SearchAsync("distributed systems", limit: 20, ct: CancellationToken.None);
```

## Notes
- Displayed URLs come from the highest-ranked provider; canonicalization (lowercase host, stripped trailing slash, removed fragments and utm_ / fbclid params) is used only for merging, not rendering. This keeps the user-facing URL stable while still enabling cross-provider deduplication.
- If a provider throws an error, the error is caught and logged; the others contribute whatever results they return. If all providers fail, the method returns an empty result list, matching the contract of a single backend.
- The per-provider limit is applied to each provider to give the merger room to detect overlaps; the merged results are trimmed down to the requested limit after merging.
