# LocalDocsOptions

> **File:** `src/api/Gabriel.Core/Configuration/LocalDocsOptions.cs`  
> **Kind:** class

```csharp
public class LocalDocsOptions : IConfigSection<LocalDocsOptions>
```


LocalDocsOptions is a small configuration container that governs whether the docs pipeline should read from a local, on-disk copy of Gabriel’s self-docs and where to find those files. When Enabled is true, the local folder specified by Path is consulted in addition to the GitHub-backed source; when false, the local source is skipped entirely. Path defaults to docs/gabriel-self-docs and accepts both forward and backward slashes; the resolver normalizes to the platform's separator. The resolution strategy described in the comments is: if Path is absolute and exists, use it as-is; otherwise, probe Environment.CurrentDirectory and AppContext.BaseDirectory upward to locate a matching folder; if nothing is found, the local source behaves as empty and a one-time warning is logged. This option centralizes local-docs configuration and lets you override or test docs locally without changing the rest of the feeds.