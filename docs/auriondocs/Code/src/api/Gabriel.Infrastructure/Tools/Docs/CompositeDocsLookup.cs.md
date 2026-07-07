# CompositeDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/CompositeDocsLookup.cs`  
> **Kind:** class

```csharp
public sealed class CompositeDocsLookup : IDocsLookup
```


CompositeDocsLookup delegates to an ordered list of inner IDocsLookup sources, treating the first as the primary and consulting later sources only when earlier ones don’t provide an answer. It merges results in a way that preserves each source’s ordering while favoring the primary source for duplicate paths.

## Remarks

Materializes the input sources into a list so registration order is preserved and the underlying IEnumerable isn’t re-enumerated on every call. ListAsync merges entries by relative path using a case-insensitive comparison and deduplicates duplicates across sources, with primary source entries winning when paths collide. If a source throws during ListAsync, the error is logged and the source is skipped; if all sources fail or return no docs, the last transient exception is rethrown to surface the root cause (e.g., rate limits, 5xx, DNS). ReadAsync, on the other hand, queries sources in order and returns the first non-null hit; input-validation or argument exceptions are surfaced immediately, while other exceptions are logged and cause the search to continue.

## Example

```csharp
// Example wiring: primary on-disk docs and fallback to GitHub-hosted docs
var composite = new CompositeDocsLookup(
    new IDocsLookup[] { localDocs, githubDocs },
    logger
);
```

## Notes

- ListAsync uses a union-by-path with case-insensitive matching; duplicates across sources are collapsed with priority to earlier sources.
- If every source returns no docs and at least one source threw, the last exception is rethrown to aid diagnosis.
- Composite has no internal caching; repeated ListAsync/ReadAsync calls will query sources anew.