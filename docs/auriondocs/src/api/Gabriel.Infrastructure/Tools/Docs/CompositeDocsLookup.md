# CompositeDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/CompositeDocsLookup.cs`  
> **Kind:** class

Composes multiple IDocsLookup implementations into a prioritized (ordered) lookup. Use this when you want a primary docs source that is consulted first and one or more fallback sources that are only consulted if earlier sources don't provide an answer.

## Remarks
CompositeDocsLookup preserves the registration order of the supplied IDocsLookup instances (the constructor materializes the IEnumerable to a list) and treats earlier sources as higher priority. For listing, it produces a union of entries keyed by DocsEntry.Path using a case-insensitive comparison; when two sources expose the same path the entry from the earlier (higher-priority) source wins and is placed earlier in the returned list. For reads it queries sources in order and returns the first non-null DocsContent.

The class is resilient to transient failures in individual sources: during ListAsync a failing source is logged and skipped so other sources can still contribute results. However, if every source either returned empty or threw, and at least one threw, the last transient exception is rethrown so callers can see the underlying cause (e.g., rate limit or network error) instead of a generic "no docs" result. Certain exceptions from inner lookups are propagated immediately (OperationCanceledException and ArgumentException) because they indicate cancellation or invalid input rather than a transient source failure.

## Example
```csharp
// Build a composite with a primary (local) source and a fallback (GitHub) source
var composite = new CompositeDocsLookup(new IDocsLookup[] { localLookup, githubLookup }, logger);

// List merged entries (primary entries will come first)
var entries = await composite.ListAsync(CancellationToken.None);

// Read a specific path using the ordered fallback behavior
var content = await composite.ReadAsync("guides/getting-started.md", CancellationToken.None);
```

## Notes
- Ordering matters: register the primary (highest-priority) source first.
- Path deduplication uses StringComparer.OrdinalIgnoreCase; paths that differ only by case are considered identical.
- ArgumentException and OperationCanceledException thrown by an inner source are propagated immediately; other exceptions are logged and treated as transient fallbacks (and may be rethrown from ListAsync if they caused a total failure).
