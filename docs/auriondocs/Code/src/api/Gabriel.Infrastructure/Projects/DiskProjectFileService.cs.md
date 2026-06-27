# DiskProjectFileService

> **File:** `src/api/Gabriel.Infrastructure/Projects/DiskProjectFileService.cs`  
> **Kind:** class

Stores project files on local disk and keeps file metadata in the ProjectFiles database table. Use this service when you need persistent file storage scoped per-project on the server's filesystem (files are placed under {Root}/{ProjectId:N}/{filename}) while still retaining metadata and access control in the database. It performs authorization checks, enforces allowed extensions and filename sanitization, and defends against path-traversal by resolving and verifying final paths before opening any file handles.

## Remarks
DiskProjectFileService is an implementation of IProjectFileService that combines on-disk storage for file contents with relational metadata in AppDbContext.ProjectFiles. It centralizes concerns common to project-scoped file storage: access control (AuthorizeAsync is invoked on public operations), filename sanitization and extension whitelisting, collision-resistant naming (a short suffix is appended when needed), and a path-verification step that ensures every file access stays within the project's directory. The class intentionally returns file streams for callers to consume (caller disposes the stream), and provides convenience helpers such as ReadTextAsync for small textual reads.

## Example
```csharp
// List files
var files = await diskService.ListAsync(projectId, ct);

// Open a file stream and ensure disposal
var (meta, stream) = await diskService.OpenAsync(projectId, fileId, ct);
using (stream)
{
    // read or copy the stream
}

// Read up to 64KB of text content (returns null for non-text types)
string? preview = await diskService.ReadTextAsync(projectId, fileId, 64 * 1024, ct);
if (preview != null)
{
    Console.WriteLine(preview);
}
```

## Notes
- OpenAsync returns a FileStream that the caller is responsible for disposing; failing to dispose will keep file handles open.
- ReadTextAsync returns null when the stored ContentType is not considered "text-like." It reads at most maxBytes and also respects the file's SizeBytes; very large files are truncated to fit an int-sized buffer (uses int.MaxValue guard).
- If metadata exists but the disk file is missing, the service logs a warning and throws NotFoundException.
- Filename collision resolution appends a short suffix to produce a final filename; this makes concurrent uploads less likely to collide but does not replace explicit transactional coordination if strict atomicity is required.
- Several helper methods (e.g. SanitizeFilename, EnsureExtensionAllowed, ResolveFilePath, EnsureWithinProjectDir, PickAvailableNameAsync, AuthorizeAsync) are used internally to enforce policies and path-safety; their behavior governs security and validation rules seen by callers.
