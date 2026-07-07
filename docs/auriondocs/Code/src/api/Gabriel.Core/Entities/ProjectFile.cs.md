# ProjectFile

> **File:** `src/api/Gabriel.Core/Entities/ProjectFile.cs`  
> **Kind:** class

```csharp
public class ProjectFile
```


ProjectFile represents metadata about a file uploaded to a project; the bytes themselves reside on disk. It captures Id, ProjectId, Name, RelativePath, SizeBytes, ContentType, and UploadedAt, and is created through the factory method to enforce invariants.

## Remarks
ProjectFile serves as a stable, immutable record of a file's metadata within a project. It decouples the storage of the actual file contents from the metadata, enabling reliable tracking, auditing, and path resolution under the project root. The factory method enforces essential invariants (non-empty project identifier, name, and relative path; non-negative size) and normalizes inputs (RelativePath uses forward slashes, Name is trimmed). Because properties have private setters, the instance becomes effectively immutable after creation, which helps preserve consistency across the system when metadata is persisted alongside the physical file.

## Example
```csharp
// Most common usage: create a metadata entry for an uploaded file
var projectId = Guid.NewGuid();
var file = ProjectFile.Create(projectId, "report.pdf", "docs/reports/2026/report.pdf", 204800, "application/pdf");
```

## Notes
- This class holds metadata only; the actual file bytes are not stored here. To access the content, read from the disk location derived from RelativePath within the project root.
- The Create method validates inputs (e.g., ProjectId cannot be empty, Name/RelativePath cannot be whitespace, SizeBytes must be non-negative) to uphold invariants at the domain boundary.
- RelativePath normalization replaces backslashes with forward slashes to ensure consistent storage and lookup.
- Id is generated automatically when the instance is created; callers cannot supply or modify it, reinforcing immutability of the metadata object.