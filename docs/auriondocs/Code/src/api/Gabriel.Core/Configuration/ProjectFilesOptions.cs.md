# ProjectFilesOptions

> **File:** `src/api/Gabriel.Core/Configuration/ProjectFilesOptions.cs`  
> **Kind:** class

```csharp
public class ProjectFilesOptions : IConfigSection<ProjectFilesOptions>
```


ProjectFilesOptions defines the policy for per-project file storage and access. It binds to the 'Projects:Files' configuration and provides a root directory for per-project subfolders, a maximum upload size, a whitelist of allowed extensions, and a set of text-content-type prefixes that ReadTextAsync will treat as readable text. The defaults (relative root './projects-data', 25 MiB limit, and conservative lists) are chosen to support developer convenience while offering basic safety in production.

## Remarks
This abstraction centralizes storage policy to separate concerns between configuration and the runtime file-handling logic, enabling deployments to adjust behavior without touching code. The Root path is designed to be overridden via the GABRIEL_PROJECTS__FILES__ROOT environment variable, facilitating environment-specific configurations without recompilation. Text readability is controlled by TextContentTypePrefixes used by ReadTextAsync; non-text types may still be downloaded, but will not be rendered as text, reducing the risk of exposing binary data in text viewers.

## Notes
- The AllowedExtensions list acts as a whitelist; uploads with extensions not listed will be rejected by validation.
- TextContentTypePrefixes governs which files ReadTextAsync will render as text; ensure content types are correctly set to enable the intended previews.
- MaxFileBytes is a boundary check to prevent oversized uploads from exhausting disk space; adjust carefully in production to balance user needs and resource protection.