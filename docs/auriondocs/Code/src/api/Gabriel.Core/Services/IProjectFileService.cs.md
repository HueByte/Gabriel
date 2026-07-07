# IProjectFileService

> **File:** `src/api/Gabriel.Core/Services/IProjectFileService.cs`  
> **Kind:** interface

```csharp
public interface IProjectFileService
```


IProjectFileService is a per-project file-management contract that provides asynchronous operations for listing, retrieving, streaming, and modifying files within a single project, while enforcing sandboxing under the project's root directory (no absolute paths or traversal outside the project). Implementations should favor streaming over loading entire files into memory and expose a clear separation between file metadata (ProjectFile) and content streams.

## Remarks

Why this abstraction exists: it centralizes concerns around project-scoped file access, authorization, and efficient I/O behind a stable API surface that controllers and tooling can rely on. OpenAsync returns both the file metadata and an open content stream that the caller must dispose; this enables low-memory downloads even for large files. ReadTextAsync provides a safe path to read text content with a maximum byte limit and returns null for non-text-like content.

## Example

```csharp
// Example: open a file and read text if possible
var (file, stream) = await projectFileService.OpenAsync(projectId, fileId, ct);
using (stream)
{
    using var reader = new StreamReader(stream, Encoding.UTF8);
    string? text = await reader.ReadToEndAsync();
    // use text if not null
}
```

## Notes

- The Content stream in OpenAsync must be disposed by the caller to release unmanaged resources.
- ReadTextAsync may return null if the file isn't text-like; handle null accordingly.
- GetProjectDirectoryAsync enforces authorization and returns the absolute on-disk directory; callers should rely on this when integrating with tooling, and avoid assuming a particular layout outside the project sandbox.