# CompositeDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/CompositeDocsLookup.cs`  
> **Kind:** class

```csharp
public sealed class CompositeDocsLookup : IDocsLookup
```


CompositeDocsLookup aggregates multiple IDocsLookup sources into a single, ordered view, honoring a defined priority. It delegates ListAsync and ReadAsync calls to its inner sources, merges results by path with earlier sources winning duplicates, and returns primary entries before fallbacks.

## Remarks

By design this abstraction avoids a total failure when one source becomes unavailable, skipping failing sources and returning whatever other sources can answer. ReadAsync continues to search in order and returns the first non-null hit; input-validation errors bubble up immediately and cancellation propagates.

## Notes

- Path comparison is case-insensitive and duplicates across sources are deduplicated by path; the first entry for a given path wins.
- If all sources fail or return empty, but at least one source threw, the last transient exception is rethrown to reveal the root cause (e.g., rate limits, DNS issues).
- The constructor materializes the provided sources into a list to preserve registration order and avoid re-enumeration on every call.