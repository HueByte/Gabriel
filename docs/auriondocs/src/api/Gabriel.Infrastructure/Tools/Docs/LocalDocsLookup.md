# LocalDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/LocalDocsLookup.cs`  
> **Kind:** class

Provides an IDocsLookup implementation that sources model-targeted markdown files from a local on-disk folder. Use this when you want the application to prefer locally authored LLM-native documentation (the primary docs source) — it enumerates .md files under the resolved root and reads individual documents as DocsContent objects.

## Remarks
This lookup performs a lazy, cached resolution of the configured local docs root on first use. If LocalDocsOptions.Path is an absolute existing path it is used directly; otherwise the resolver probes Environment.CurrentDirectory and AppContext.BaseDirectory and walks up a limited number of parent directories (MaxParentLevels) to find the relative path. If no match is found the lookup behaves as empty so a composite lookup can fall back to a remote/GitHub source. The listing normalizes returned paths to use forward slashes and attempts to populate DocsEntry.Title by scanning for the first H1 within the early bytes of each file. ReadAsync defends against traversal and symlink surprises by combining and resolving full paths and ensuring the final file path starts under the resolved root before reading.

## Example
```csharp
// Typical usage: list available docs and read one entry
var entries = await localDocsLookup.ListAsync(CancellationToken.None);
if (entries.Count > 0)
{
    var first = entries[0];
    var content = await localDocsLookup.ReadAsync(first.Path, CancellationToken.None);
    if (content != null)
    {
        Console.WriteLine($"Title: {first.Title}");
        Console.WriteLine(content.Content);
    }
}
```

## Notes
- The lookup honors LocalDocsOptions.Enabled; when disabled it behaves as empty (ListAsync returns an empty list and ReadAsync returns null).
- Title extraction only scans a limited number of bytes (TitleScanByteLimit), so very large front-matter or nonstandard formats can cause the H1 title to be missed.
- ReadAsync returns null for invalid or out-of-root paths and for files that do not exist; callers should handle null rather than expecting exceptions for these cases.