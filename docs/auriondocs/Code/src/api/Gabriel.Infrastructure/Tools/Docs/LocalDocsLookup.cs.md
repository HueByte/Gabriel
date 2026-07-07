# LocalDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/LocalDocsLookup.cs`  
> **Kind:** class

```csharp
public sealed class LocalDocsLookup : IDocsLookup
```


LocalDocsLookup is a concrete implementation of IDocsLookup that sources model documentation from a local on-disk self-docs folder. It serves as the primary docs source for the running application, taking precedence over the GitHub-backed fallback when enabled, so the model can access its own documentation without network access.

It resolves the root path lazily on first use by honoring LocalDocsOptions.Path if absolute and existing; otherwise it walks upward from the current directory and AppContext.BaseDirectory up to eight levels to locate a matching root. If none is found, it behaves as though there are no docs.

ListAsync enumerates every markdown file under the resolved root, producing DocsEntry items with a path relative to the root, a title parsed from the first H1 in the file, and a source marker of LocalLlmNative. ReadAsync loads a single document if the path is valid and within the root, returning a DocsContent with the file contents, a canonical URL, and LocalLlmNative as the source.

## Remarks
LocalDocsLookup isolates filesystem-based docs from the rest of the docs pipeline, enabling fast model access to its own self-documentation and ensuring offline behavior. It cooperates with the overall docs infrastructure by exposing content that can be consumed by the same UI or tooling that consumes GitHub-hosted docs; when local docs are disabled or missing, the composite lookup can transparently fall back to GitHub sources. The lazy root resolution and explicit containment checks improve startup performance and security by avoiding unnecessary IO and preventing traversal beyond the configured root.

## Notes
- If LocalDocsOptions.Path is not configured or the resolved root cannot be found, ListAsync returns an empty set and ReadAsync returns null; expect the system to fall back to remote sources via the composite lookup.
- Titles are derived from the first H1 in each Markdown file; if no H1 is present, the title may be null, which is acceptable for non-title-bearing entries.
