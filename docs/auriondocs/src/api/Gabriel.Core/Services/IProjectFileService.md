# IProjectFileService

> **File:** `src/api/Gabriel.Core/Services/IProjectFileService.cs`  
> **Kind:** interface

Provides project-scoped file operations (list, read, open, upload, delete) with built-in path-hardening and authorization. Use this when interacting with files owned by a specific project so callers get a sandboxed, authenticated view of on-disk content without implementing their own path traversal checks or authorization.

## Remarks
This interface centralizes file access rules for project assets: it enforces that paths resolve under the project's root (no `..` or absolute paths), performs authorization for project access, and exposes both metadata and streaming APIs so large files can be handled without buffering them entirely in memory. Controllers and agent filesystem tools consume this abstraction to keep authz, sandboxing, and content-type handling consistent across the application.

## Example
```csharp
// Uploading a file (controller/action):
using (var stream = file.OpenReadStream())
{
    var uploaded = await projectFileService.UploadAsync(projectId, file.FileName, file.ContentType, stream, ct);
    // uploaded contains the saved file metadata
}

// Downloading a file (controller/action):
var (meta, contentStream) = await projectFileService.OpenAsync(projectId, fileId, ct);
try
{
    // copy to response, etc.
    await contentStream.CopyToAsync(responseBodyStream, ct);
}
finally
{
    contentStream.Dispose(); // caller must dispose the stream returned by OpenAsync
}

// Reading a text preview (tool):
var text = await projectFileService.ReadTextAsync(projectId, fileId, maxBytes: 64 * 1024, ct);
if (text == null)
{
    // not text-like or disallowed by content type
}
else
{
    // process text
}
```

## Notes
- OpenAsync returns an open Stream that the caller MUST dispose to avoid leaking file handles.
- ReadTextAsync deliberately refuses non-text content types and returns null for non-text-like files; it also limits the read to the provided maxBytes to avoid large memory use.
- GetProjectDirectoryAsync performs authorization and will throw if the current user cannot access the project; callers should not duplicate authorization checks when using it.
- All methods accept a CancellationToken — propagate a token to allow cooperative cancellation of long-running IO operations.