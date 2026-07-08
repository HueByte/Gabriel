# LocalDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/LocalDocsLookup.cs`  
> **Kind:** class

```csharp
public sealed class LocalDocsLookup : IDocsLookup
```


LocalDocsLookup is a local, on-disk implementation of IDocsLookup that serves Markdown documentation from a configured folder, taking precedence over the GitHub-backed fallback when enabled. It lazily resolves the root path and then enumerates or fetches docs relative to that root, normalizing paths with forward slashes for consistent cataloging. Titles are derived from the first H1 header in each file, and content reads return a DocsContent with a canonical URL and the LocalLlmNative source tag.

## Remarks
LocalDocsLookup acts as the primary source of self-hosted documentation for the model when LocalDocsOptions.Enabled is true. It uses the same DocsEntry/DocsContent types as the rest of the docs system, so catalogs and readers stay consistent whether the source is local or GitHub. Root resolution is performed once (thread-safely) and cached, so the first ListAsync/ReadAsync call triggers discovery and subsequent calls reuse the computed root. If the root cannot be resolved or the feature is disabled, ListAsync yields an empty set and ReadAsync returns null, allowing a graceful fallback to the GitHub source.

## Example
```csharp
// Example usage (assumes an initialized LocalDocsLookup named `lookup`)
var entries = await lookup.ListAsync(CancellationToken.None);
foreach (var e in entries)
{
    Console.WriteLine($"{e.Path} -> {e.Title}");
}

// Read a specific document
var content = await lookup.ReadAsync("intro/getting-started.md", CancellationToken.None);
if (content is not null)
{
    Console.WriteLine(content.Content.Substring(0, 120));
}
```

## Notes
- Title may be null if the first H1 isn't found in a file; consumers should handle null Title gracefully.
- Root resolution is lazy and cached; changes to LocalDocsOptions.Path won't take effect until the instance is recreated (or the app restarts).
