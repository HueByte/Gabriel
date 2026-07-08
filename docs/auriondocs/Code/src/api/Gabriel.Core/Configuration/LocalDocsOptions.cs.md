# LocalDocsOptions

> **File:** `src/api/Gabriel.Core/Configuration/LocalDocsOptions.cs`  
> **Kind:** class

```csharp
public class LocalDocsOptions : IConfigSection<LocalDocsOptions>
```


LocalDocsOptions provides the configuration for Gabriel's on-disk self-docs source used by docs_list/docs_read. It exposes a toggle (Enabled) and a Path to locate markdown entries; the resolver tries an absolute existing path first, then searches upward from the application directory, and if nothing is found, falls back to the GitHub-backed source.

## Remarks
LocalDocsOptions serves as a focused abstraction that decouples the docs resolution from the rest of the system and centralizes local-doc resolution behind a single configuration point. It enables easy toggling of the local source for development or testing while allowing the production pipeline to rely on GitHub content when local docs are unavailable. The SectionName binding (Tools:Docs:Local) ties this configuration into the broader options framework.

## Notes
- Disabling local docs via Enabled=false skips the local path resolution entirely; the docs pipeline will consult GitHub sources only.
- Path can be absolute or relative; relative paths are resolved using the same lookup strategy and are normalized to the platform's path separator.
- If no matching path is found, the local source is treated as empty and a one-time warning is logged; the composite lookup then uses the GitHub-backed docs.