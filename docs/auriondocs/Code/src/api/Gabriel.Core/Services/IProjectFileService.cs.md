# IProjectFileService

> **File:** `src/api/Gabriel.Core/Services/IProjectFileService.cs`  
> **Kind:** interface

```csharp
public interface IProjectFileService
```


Defines asynchronous operations to manage files within a single project: listing file metadata, retrieving file details, opening an open content stream for downloads, reading text content up to a limit, uploading new content from a stream, deleting files, and obtaining the project's sandbox directory. It hardens against path traversal and avoids loading whole files into memory by streaming where possible; consumers such as web controllers and tooling can perform file operations without buffering large payloads.

## Remarks
Architecturally, this interface centralizes filesystem concerns for a given project, pairing metadata with content streaming behind a single abstraction. It ensures authorization and sandbox scoping (via GetProjectDirectoryAsync) are performed in one place, enabling swap-out storage implementations without changing callers. The streaming contracts (OpenAsync returning a Content stream, and ReadTextAsync returning null for non-text) shape how consumers should interact with large or binary files.

## Example
```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public async Task PreviewFirstFileAsync(IProjectFileService service, Guid projectId, CancellationToken ct)
{
    var files = await service.ListAsync(projectId, ct);
    var first = files.FirstOrDefault();
    if (first != null)
    {
        var maxBytes = 1024 * 1024; // 1 MB
        var text = await service.ReadTextAsync(projectId, first.Id, maxBytes, ct);
        if (text != null)
        {
            Console.WriteLine(text);
        }
    }
}
```

## Notes
- ReadTextAsync may return null if the file isn't text-like; handle nulls gracefully.
- OpenAsync returns a tuple (File, Content). The Content stream must be disposed by the caller after use.
- UploadAsync and ReadTextAsync operate on streams; ensure streams are properly disposed and avoid buffering entire content in memory when possible. Path handling is sandboxed to the project's directory to prevent traversal outside the project root.