# DiskProjectFileService

> **File:** `src/api/Gabriel.Infrastructure/Projects/DiskProjectFileService.cs`  
> **Kind:** class

Stores and retrieves project file content on the local disk while keeping file metadata in the ProjectFiles database table. Use this implementation when you want files laid out on the server filesystem under {Root}/{ProjectId:N}/ with the service enforcing authorization, filename sanitization, extension checks, collision handling, and path-traversal protection before opening any file handles.

## Remarks
This class implements IProjectFileService for a disk-backed storage strategy: the database holds metadata (ProjectFile entries) and the file bytes live on disk. Before performing operations it verifies the current caller is authorized (AuthorizeAsync) and it resolves and validates every filesystem path so that no operation can escape the per-project directory. It also applies a deterministic collision policy on upload (appends a short suffix when a sanitized filename is already taken), making concurrent uploads safe and the final filename predictable from the returned ProjectFile.

## Example
```csharp
// Resolve from DI
var service = serviceProvider.GetRequiredService<IProjectFileService>();

// Read a text preview (returns null for binary content)
string? preview = await service.ReadTextAsync(projectId, fileId, maxBytes: 32_768);

// Open the file stream for streaming download - caller must dispose
var (meta, stream) = await service.OpenAsync(projectId, fileId);
using (stream)
{
    // copy to response, process, etc.
}

// Uploading a new file
using var contentStream = File.OpenRead("local-path/to/upload.txt");
var created = await service.UploadAsync(projectId, "upload.txt", "text/plain", contentStream);
// created.RelativePath contains the actual filename chosen on disk
```

## Notes
- OpenAsync returns a FileStream that the caller is responsible for disposing; failing to dispose will keep file handles open.
- ReadTextAsync only attempts to read when the stored ContentType is considered text-like and will read at most the provided maxBytes (and at most the file's SizeBytes); it returns null for non-text content.
- UploadAsync sanitizes the supplied filename and may change it (appending a short suffix) to avoid collisions — always use the returned ProjectFile to learn the final stored name. If metadata exists but the disk file is missing, the service logs a warning and throws NotFoundException when attempting to open the missing file.
