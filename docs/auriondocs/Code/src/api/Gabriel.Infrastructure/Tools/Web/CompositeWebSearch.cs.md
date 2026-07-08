# CompositeWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/CompositeWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class CompositeWebSearch : IWebSearch
```


CompositeWebSearch acts as an IWebSearch that fans out a query to multiple backing providers in parallel and merges their results into a single ranked list. This approach provides broader coverage and a cross-provider signal, improving the chances of surfacing relevant results compared to using any single provider alone. The merge groups results by a canonicalized URL and assigns a score of appearance_count * 1000 - min_rank_across_providers, so cross-provider hits outrank single-provider hits and, within the same appearance count, the highest-ranked hit wins. When duplicates are detected, the first (highest-ranked) raw URL seen is preserved for display, while the Title and Snippet fields are chosen via the described merging rules (best rank, longest non-empty snippet). Each provider is queried with the requested limit and results are merged before trimming back to that limit. Errors from individual providers are caught with a logged warning and do not poison the others; if all providers fail, a zero-result list is surfaced to match the contract of any single backend. The implementation executes per-provider calls in parallel and then aggregates results in memory to produce the final set.

Key points captured by this symbol include:
- Parallel execution of providers and per-provider error isolation.
- Canonical URL grouping for deduplication without altering the display URL.
- A simple yet effective cross-provider ranking that favors multi-provider signals.
- Safe handling of partial failures with graceful degradation.
