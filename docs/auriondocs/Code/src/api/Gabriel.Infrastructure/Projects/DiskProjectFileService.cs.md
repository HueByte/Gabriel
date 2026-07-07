# DiskProjectFileService

> **File:** `src/api/Gabriel.Infrastructure/Projects/DiskProjectFileService.cs`  
> **Kind:** class

*Figure: How DiskProjectFileService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
DiskProjectFileService["DiskProjectFileService: entry (List/Get/Open/ReadText)"]
DiskProjectFileService --> IProjectRepository["AuthorizeAsync(projectId) via ICurrentUser"]
IProjectRepository --> AppDbContext["Query AppDbContext.ProjectFiles for fileId and projectId"]
AppDbContext --|"no match"| NotFoundException["Throw NotFoundException(nameof(ProjectFile), fileId)"]
AppDbContext --|"match"| ProjectFile["Load ProjectFile metadata (SizeBytes, RelativePath, ContentType)"]
ProjectFile --> ProjectFilesOptions["ProjectFilesOptions: Root path config"]
ProjectFile --|"resolve path"| DiskProjectFileService
DiskProjectFileService --|"ResolveFilePath and verify final path is inside project dir"| DiskProjectFileService
DiskProjectFileService --|"disk file missing -> log warning & throw"| NotFoundException
DiskProjectFileService --|"disk file exists -> open FileStream and return (ProjectFile, Stream)"| ProjectFile
ProjectFile --|"OpenAsync returns (ProjectFile, Stream)"| DiskProjectFileService
DiskProjectFileService --|"IsTextLike(ContentType)? -> no -> return null"| ProjectFile
DiskProjectFileService --|"IsTextLike(ContentType)? -> yes"| DiskProjectFileService
DiskProjectFileService --|"Read up to cap bytes in loop, UTF8-decode and return string"| ProjectFile
```

```csharp
public sealed class DiskProjectFileService : IProjectFileService
```


Stores and retrieves project files on local disk while keeping file metadata in the ProjectFiles database table. Use this service when you want durable file storage colocated with the application process and need built-in safeguards (authorization checks, allowed extensions, per-project directories and path-traversal protection) rather than implementing file-on-disk logic yourself.

## Remarks
DiskProjectFileService centralizes two concerns: persistent metadata (kept in the ProjectFiles table) and the actual file contents (kept under {Root}/{ProjectId:N}/). Every public operation enforces project-level authorization and validates disk access by resolving the final path and verifying it sits inside the project's directory to mitigate path-traversal attacks. Uploads sanitize filenames, validate extensions against configured allowed lists, create the project directory if needed, and pick a non-colliding filename (a short suffix is appended when necessary) so concurrent uploads remain predictable.

## Notes
- OpenAsync returns a FileStream the caller is responsible for disposing; failing to dispose will keep the file handle open.
- ReadTextAsync will return null when the file's ContentType is not considered text-like; it also caps the read to the provided maxBytes and to the file's recorded SizeBytes.
- Upload enforces allowed extensions and a configured maximum size; violations surface as domain/validation exceptions (e.g. DomainException) or other appropriate errors.
- If metadata exists but the backing disk file is missing, operations that open the file will log a warning and throw NotFoundException for the ProjectFile.
