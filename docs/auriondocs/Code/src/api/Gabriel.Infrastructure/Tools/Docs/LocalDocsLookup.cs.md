# LocalDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/LocalDocsLookup.cs`  
> **Kind:** class

```csharp
// IDocsLookup over the on-disk LLM-native self-docs folder. This is the
// PRIMARY docs source - pages here are written specifically for the model to
// consume and take precedence over the GitHub-backed fallback.
//
// Root resolution runs once on first use (lazy). When LocalDocsOptions.Path
// is absolute and exists, it is used as-is. Otherwise the resolver probes
// Environment.CurrentDirectory and AppContext.BaseDirectory, walking up a few
// parents looking for the relative path. First match wins. If nothing is
// found the source behaves as empty (zero entries, all reads return null) and
// the composite lookup will transparently fall back to the GitHub source.
public sealed class LocalDocsLookup : IDocsLookup
```


Provides an on-disk IDocsLookup implementation that reads LLM-targeted markdown pages from a local repository folder. Use this when you want the model to prefer locally authored, LLM-native documentation files (the primary docs source) instead of the GitHub-backed fallback; it enumerates .md files, extracts a first H1 for each entry's title, and serves file contents on demand.

## Remarks
LocalDocsLookup is the primary, file-system-backed docs source used by the composite lookup system. It performs a lazy root-resolution on first use (protected by a lock) and then lists or reads markdown files beneath that root. If LocalDocsOptions.Path is an absolute path that exists it will be used directly; otherwise the resolver probes Environment.CurrentDirectory and AppContext.BaseDirectory and walks upward a limited number of parent directories looking for the configured relative path (first match wins). If no root is found the lookup behaves as empty so higher-level components can transparently fall back to the remote/GitHub source.

## Example
```csharp
// Using the lookup directly (commonly resolved from DI):
var entries = await localDocsLookup.ListAsync(CancellationToken.None);
foreach (var e in entries)
{
    Console.WriteLine($"{e.Path} - {e.Title}");
}

// Read a specific page (path uses forward slashes relative to the docs root):
var content = await localDocsLookup.ReadAsync("guides/getting-started.md", CancellationToken.None);
if (content != null)
{
    Console.WriteLine(content.Text);
}
```

## Notes
- If LocalDocsOptions.Enabled is false, ListAsync returns an empty list and ReadAsync returns null; the lookup is effectively disabled.
- Title extraction only scans the first TitleScanByteLimit bytes (4096) for the first H1 (`# ...`); leading BOM and blank lines are tolerated but very long initial front matter or missing headings yield a null Title.
- ReadAsync normalizes separators and re-anchors the requested path against the resolved root and rejects requests that resolve outside the root (defense against path traversal); it also returns null when the file does not exist.
- Paths returned by ListAsync are relative to the docs root and use forward slashes so they round-trip correctly across platforms.