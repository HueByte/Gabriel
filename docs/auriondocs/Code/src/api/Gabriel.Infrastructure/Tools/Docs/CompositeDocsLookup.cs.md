# CompositeDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/CompositeDocsLookup.cs`  
> **Kind:** class

```csharp
public sealed class CompositeDocsLookup : IDocsLookup
```


CompositeDocsLookup is an IDocsLookup implementation that fans across an ordered collection of inner sources, treating the first as the primary source and the rest as fallbacks. It presents a unified view by merging results from all sources, while honoring the priority of the primary source and preserving each source's internal ordering. When listing documentation entries, paths are deduplicated in a case-insensitive manner; the first source that exposes a given path wins, and entries from the primary source appear before those from any fallbacks.

Read operations are attempted sequentially against the inner sources in order. The first non-null DocsContent returned wins. Transient failures from individual sources (e.g., network hiccups or API rate limits) are logged and do not poison the whole lookup; the last transient exception is tracked and, if every source yields no result, is rethrown to surface the underlying cause to the caller. This design enables robust partial availability: if one source is down or slow, others may still provide helpful results.

The constructor materializes the incoming enumerable into a list to preserve registration order and to avoid re-enumeration on every call, ensuring predictable and efficient behavior. The class is sealed and uses lightweight local state during operations, making it suitable for concurrent use by multiple callers.
