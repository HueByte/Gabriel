# CompositeWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/CompositeWebSearch.cs`  
> **Kind:** class

Fans a single query out to multiple IWebSearch providers in parallel, merges their results into a single ranked list, and returns the top items. Use this when you want to combine coverage and cross-provider signal (a URL returned by several providers is treated as stronger) rather than relying on one backend.

## Remarks
The class parallelizes queries to each configured provider, requests up to the caller-specified limit from every provider (so the merge can discover cross-provider overlaps), then groups results by a canonicalized URL for de-duplication. It scores merged hits by appearance count and best provider rank (appearance_count * 1000 - min_rank) so results returned by multiple providers surface above single-provider hits; within the same appearance count, the best-ranked occurrence wins. Title selection favors the provider that ranked the URL highest; snippet selection picks the longest non-empty snippet across providers. Errors from an individual provider are logged and do not prevent merging results from the others; if every provider fails the caller receives an empty list, matching the contract of a single backend.

## Example
```csharp
// Combine two search backends and fetch the top 10 merged results
var providers = new IWebSearch[] { new ProviderA(), new ProviderB() };
var composite = new CompositeWebSearch(providers, logger);
var results = await composite.SearchAsync("open source libraries", limit: 10, CancellationToken.None);
foreach (var r in results)
{
    Console.WriteLine($"{r.Title} - {r.Url}");
}
```

## Notes
- The constructor requires at least one provider and throws ArgumentException if the providers list is empty.
- The provided limit is applied per provider when fetching; the merged result set is trimmed back to the requested limit after ranking.
- Canonicalization is used only for grouping/matching; the first-seen (highest-ranked) raw URL is preserved for display.
- The scoring uses a fixed multiplier (1000) under the assumption no provider returns more than 1000 results; changing that assumption may require adjusting the constant.
- CancellationToken provided to SearchAsync is forwarded to provider calls; individual provider failures are caught and logged rather than propagated.