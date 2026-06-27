# CompositeDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/CompositeDocsLookup.cs`  
> **Kind:** class

Composes multiple IDocsLookup implementations into a single lookup that consults sources in a defined priority order. The first source in the provided sequence is treated as primary (its entries are listed first and its reads win); subsequent sources act as fallbacks and are consulted only when earlier sources don't provide an answer. Use this when you want a single logical docs provider backed by multiple physical stores (for example, local disk first, then remote GitHub).

## Remarks
This class preserves the registration order of the provided sources by materializing the IEnumerable into a list at construction time; ordering is significant — the first source is the highest priority. For listing, entries from multiple sources are merged by Path using a case-insensitive comparison so that duplicates from lower-priority sources are suppressed. Read operations consult each source in order and return the first non-null result. Transient failures from an individual source are logged and do not by themselves fail the overall operation, allowing other sources to answer; however, ListAsync records the last transient exception and will rethrow it if every source either returned no entries or threw, ensuring callers see a real underlying cause (rate limit, 5xx, DNS) instead of a generic empty result.

## Example
```csharp
// Typical wiring: primary first, then fallback(s)
IDocsLookup local = new LocalDocsLookup("./docs");
IDocsLookup github = new GitHubDocsLookup("my-org", "repo");
ILogger<CompositeDocsLookup> logger = loggerFactory.CreateLogger<CompositeDocsLookup>();

var composite = new CompositeDocsLookup(new[] { local, github }, logger);

// List merged entries (local entries appear first; duplicate paths from github are ignored)
var list = await composite.ListAsync(CancellationToken.None);

// Read a specific doc; returns the first non-null content from local then github
var content = await composite.ReadAsync("guides/getting-started.md", CancellationToken.None);
```

## Notes
- Ordering matters: always pass the primary (highest-priority) source first. The class does not reorder sources.
- Entry uniqueness is determined by DocsEntry.Path using a case-insensitive comparer; lower-priority duplicates are skipped.
- ListAsync will rethrow a transient exception only when the merged result is empty and at least one source threw; otherwise transient errors are logged and skipped.
- ReadAsync propagates OperationCanceledException and ArgumentException from inner sources immediately; other exceptions are logged and treated as transient (the source is skipped).
- The CompositeDocsLookup itself is effectively immutable after construction, but thread-safety depends on the thread-safety of the supplied IDocsLookup implementations.