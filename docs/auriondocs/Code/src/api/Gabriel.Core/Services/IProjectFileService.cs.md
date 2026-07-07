# IProjectFileService

> **File:** `src/api/Gabriel.Core/Services/IProjectFileService.cs`  
> **Kind:** interface

```csharp
public interface IProjectFileService
```


IProjectFileService defines the asynchronous contract for managing a project's uploaded files. It exposes operations to list, inspect metadata, stream-into-downloading content, read text safely, upload new files, delete existing ones, and locate the absolute directory where a project's files live, all scoped to a single project to enforce isolation.

## Remarks
By centralizing file I/O behind this interface, implementations can enforce authorization, per-project sandboxing, and streaming semantics that avoid loading entire files into memory. OpenAsync returns a tuple of metadata and an open Stream that the caller must dispose, which enables efficient downloads and streaming tools.

## Notes
- The Content stream returned by OpenAsync must be disposed by the caller to release file handles and resources.
- ReadTextAsync returns null if the file content is not text-like, so callers should handle null accordingly.
