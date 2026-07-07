# LocalDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/LocalDocsLookup.cs`  
> **Kind:** class

```csharp
public sealed class LocalDocsLookup : IDocsLookup
```


LocalDocsLookup is a lazy, on-disk provider of model-local documentation that implements IDocsLookup. It scans a configured root directory for Markdown files and exposes them as DocsEntry items with paths relative to the root and titles derived from the first H1 in each file; ReadAsync returns the file content along with a canonical URL, while failing gracefully when local docs are disabled or unavailable.

## Remarks
LocalDocsLookup acts as the primary local source of docs, taking precedence over the GitHub-backed fallback when present. It resolves its root lazily on first use by honoring an absolute, existing path or by walking up to eight directory levels from the current process to locate a relative path. The root, once resolved, is cached for subsequent operations, and path validation prevents traversal outside the root boundary. If the configured root cannot be found, the lookup yields no entries and ReadAsync returns null, allowing the composite docs source to fall back to GitHub.

## Notes
- Root resolution is cached after the first use; changing LocalDocsOptions.Path at runtime won't trigger a re-resolution.
- Title extraction uses the first H1, tolerating a BOM and blank lines; if no H1 exists, the DocsEntry.Title may be null.
- The lookup enforces a strict boundary: the combined path must reside under the resolved root; otherwise ReadAsync returns null or ListAsync yields no entries.