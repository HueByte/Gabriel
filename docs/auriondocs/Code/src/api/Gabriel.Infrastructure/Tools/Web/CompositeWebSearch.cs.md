# CompositeWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/CompositeWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class CompositeWebSearch : IWebSearch
```


CompositeWebSearch fans a query out to multiple backing providers in parallel and merges their results into a single, deduplicated list. Use it when you want broader coverage and a cross-provider signal rather than relying on a single backend; the implementation canonicalizes URLs for grouping, scores results by appearances and the best rank seen across providers, and preserves the highest-ranked raw URL for display. If a provider fails, its results are still merged (with a logged warning); if all providers fail, a zero-result list is surfaced to satisfy the contract of a single-backend query. Each provider receives the same per-provider limit, and the final merged results are trimmed to the requested limit after aggregation. The constructor enforces at least one provider, throwing an ArgumentException when given an empty list.