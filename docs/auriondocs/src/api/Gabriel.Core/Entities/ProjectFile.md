# ProjectFile

> **File:** `src/api/Gabriel.Core/Entities/ProjectFile.cs`  
> **Kind:** class

Represents metadata for a file uploaded into a project. Use this class whenever you need a compact, immutable description of an uploaded file (its user-facing name, on-disk relative path, size, content type and timestamps) rather than the file bytes themselves; the actual bytes are stored on disk under {ProjectsRoot}/{ProjectId:N}/{RelativePath}.

## Remarks
ProjectFile is an immutable-like entity with a private constructor and a single factory method (Create) that enforces basic validation and normalization. Id is generated automatically, UploadedAt defaults to UtcNow when the instance is created, and setters are private to prevent accidental mutation after creation. Path traversal and deeper filesystem validation are intentionally left to the service layer; this class only records the descriptive shape (it normalizes backslashes to forward slashes but does not remove or validate `..` segments).

## Example
```csharp
var projectId = Guid.Parse("c56a4180-65aa-42ec-a945-5fd21dec0538");
var file = ProjectFile.Create(
    projectId: projectId,
    name: "photo.png",
    relativePath: "images/photo.png",
    sizeBytes: 324_532,
    contentType: "image/png"
);

// file.Id, file.ProjectId, file.Name, file.RelativePath, file.SizeBytes, file.ContentType, file.UploadedAt
```

## Notes
- Create throws ArgumentException for invalid inputs: empty projectId, blank name, blank relativePath, or negative sizeBytes.
- RelativePath is normalized by replacing backslashes with '/' but the class does not sanitize `..` segments — ensure the service layer enforces path traversal rules before persisting or constructing ProjectFile.
- If contentType is null or whitespace, it defaults to "application/octet-stream".