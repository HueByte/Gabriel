# ProjectFile

> **File:** `src/api/Gabriel.Core/Entities/ProjectFile.cs`  
> **Kind:** class

Represents metadata for a file uploaded to a project — the descriptive record stored in the database while the actual file bytes live on disk under {ProjectsRoot}/{ProjectId:N}/{RelativePath}. Use this entity when you need to persist or transfer information about an uploaded file (identity, project association, on-disk relative path, size, content type and upload timestamp) without including the file contents.

## Remarks
ProjectFile is a small immutable-ish entity: construction is controlled via the static Create factory so callers get validated, normalized values and cannot create an inconsistent instance via public setters. The class intentionally separates file metadata from file storage; actual bytes are stored on disk and any path traversal or root-resolution checks are the responsibility of higher-level services.

## Example
```csharp
var file = ProjectFile.Create(
    projectId: project.Id,
    name: "design-doc.pdf",
    relativePath: "docs/design-doc.pdf",
    sizeBytes: 245612,
    contentType: "application/pdf"
);

Console.WriteLine(file.Id);           // GUID assigned automatically
Console.WriteLine(file.RelativePath); // "docs/design-doc.pdf"
Console.WriteLine(file.UploadedAt);   // timestamp set on creation
```

## Notes
- RelativePath is normalized by replacing backslashes with '/' and path-traversal/root enforcement is done at the service layer — callers should not assume the class sanitizes or validates against ".." segments.
- Create validates required fields: ProjectId must not be Guid.Empty, Name and RelativePath must be non-empty/whitespace, and SizeBytes must be non-negative; ContentType falls back to "application/octet-stream" when null/whitespace.
- Id and UploadedAt are assigned automatically and exposed as read-only (private setters); the object is intended to be created via the factory to preserve these invariants.
```