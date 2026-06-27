# IProjectFileService

> **File:** `src/api/Gabriel.Core/Services/IProjectFileService.cs`  
> **Kind:** interface

Provides project-scoped file storage and retrieval operations (list, get, open/download, read-as-text, upload and delete) with hardened path handling and built-in authorization. Use this service when you need to manage files that belong to a specific project while avoiding in-memory file buffering and ensuring reads/writes are constrained to the project sandbox.

## Remarks
This abstraction centralizes project-level file access rules: callers get streaming uploads/downloads to avoid loading whole files into memory, and all path handling is validated so paths cannot escape the project's directory (no `..`, no absolute paths; paths must resolve under the project's root). GetProjectDirectoryAsync also performs authorization for the current user and returns the absolute on-disk directory for the project's uploads, allowing agents or tools to operate on a safe, project-scoped sandbox without re-checking permissions.

## Example
```csharp
// Downloading: remember to dispose the returned stream
var (fileMeta, contentStream) = await projectFileService.OpenAsync(projectId, fileId, ct);
using (contentStream)
{
    // copy to HTTP response, write to disk, etc.
    await contentStream.CopyToAsync(responseStream, ct);
}

// Reading as text (may return null if the file isn't text-like)
string? text = await projectFileService.ReadTextAsync(projectId, fileId, maxBytes: 100_000, ct);
if (text == null)
{
    // handle binary/non-text file
}

// Uploading from an incoming stream without buffering whole file in memory
using var fileStream = File.OpenRead(localPath);
var uploaded = await projectFileService.UploadAsync(projectId, "notes.txt", "text/plain", fileStream, ct);

// Getting the on-disk project directory (this call enforces authz)
string projectDir = await projectFileService.GetProjectDirectoryAsync(projectId, ct);
```

## Notes
- The Stream returned by OpenAsync is an open, synchronous Stream that the caller MUST dispose; failing to do so can leak file handles.
- ReadTextAsync enforces text-like content types and reads up to the supplied maxBytes; it returns null for non-text-like files (and will not attempt to load large binary blobs into memory).
- GetProjectDirectoryAsync performs authorization and will throw if the current user is not allowed to access the project; callers should not assume it always succeeds.