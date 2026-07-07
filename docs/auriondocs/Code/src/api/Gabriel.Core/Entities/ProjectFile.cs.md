# ProjectFile

> **File:** `src/api/Gabriel.Core/Entities/ProjectFile.cs`  
> **Kind:** class

```csharp
public class ProjectFile
```


Represents metadata for a file uploaded to a project; the actual bytes live on disk at a project-scoped path, while this object stores only the descriptive metadata. Use ProjectFile.Create to construct a valid instance, which enforces required inputs (ProjectId non-empty, Name non-empty, RelativePath non-empty, SizeBytes non-negative) and normalizes values (Name trimmed, RelativePath uses forward slashes) and applies a default ContentType when needed.

## Remarks
ProjectFile is a small, immutable-like value object that records what you need to know about a file without holding onto its bytes. Id is generated automatically at instantiation and UploadedAt defaults to UTC now; the constructor is private to require creation via Create, which preserves invariants. By separating metadata from the on-disk content, this type supports efficient listing, auditing, and retrieval of file information, while coordinating with the storage layer that manages the actual bytes.

## Notes
- Create validates inputs and will throw ArgumentException when ProjectId is Guid.Empty, Name or RelativePath are blank, or SizeBytes is negative.
- RelativePath normalization converts backslashes to forward slashes to ensure consistent storage across platforms.
- ContentType defaults to application/octet-stream when not provided.