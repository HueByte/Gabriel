# LocalDocsOptions

> **File:** `src/api/Gabriel.Core/Configuration/LocalDocsOptions.cs`  
> **Kind:** class

```csharp
public class LocalDocsOptions : IConfigSection<LocalDocsOptions>
```


LocalDocsOptions is a configuration container that controls the on-disk local documentation source used by Gabriel's LLM-native self-docs. It maps to the Tools:Docs:Local configuration section and exposes two settings: Enabled and Path. When Enabled is true (default), the local Markdown docs found under Path are consulted before the GitHub-backed docs; when false, the system bypasses the local source entirely and uses GitHub docs only. Path designates the folder to read .md files from; it defaults to docs/gabriel-self-docs and accepts either forward or back slashes, with the resolver normalizing to the platform's path separator. The resolution logic proceeds as follows: if Path is absolute and exists, that path is used as-is; otherwise the resolver probes Environment.CurrentDirectory and AppContext.BaseDirectory, walking up a few parent directories until it finds a matching Path. If nothing is found, the local source behaves as empty and logs a one-time warning; the composite lookup then transparently falls back to GitHub.

## Remarks
LocalDocsOptions centralizes a pluggable, local-first documentation mechanism that lets developers override or extend the shared docs without relying exclusively on GitHub. By enabling this option, teams can ship and test self-docs alongside the application, or operate in environments without network access. The lookup strategy ensures predictable behavior: preferring the local folder when present and gracefully falling back to GitHub when it is not.

## Notes
- A one-time warning is logged if no matching local path is found, so check logs when enabling local docs to confirm the path resolution worked.
- Relative Path resolution depends on runtime context (Environment.CurrentDirectory, AppContext.BaseDirectory); place the docs so that one of these roots can reach Path.
- When Enabled is false, local docs are skipped entirely and only GitHub docs are consulted; ensure GitHub docs cover your needs in that deployment.