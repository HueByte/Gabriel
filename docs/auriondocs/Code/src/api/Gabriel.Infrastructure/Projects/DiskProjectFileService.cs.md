# DiskProjectFileService

> **File:** `src/api/Gabriel.Infrastructure/Projects/DiskProjectFileService.cs`  
> **Kind:** class

*Figure: How DiskProjectFileService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
step1["DiskProjectFileService: receive call; call AuthorizeAsync(projectId)"]
step2["Query AppDbContext.ProjectFiles for ProjectId & fileId"]
checkFound{"ProjectFile found?"}
notFound["Throw NotFoundException(ProjectFile, fileId)"]
resolvePath["ResolveFilePath(projectId, ProjectFile.RelativePath) using ProjectFilesOptions; verify inside project dir"]
checkDisk{"File.Exists(fullPath)?"}
missingOnDisk["Log warning and throw NotFoundException(ProjectFile, fileId)"]
openStream["Open FileStream(fullPath) and return (ProjectFile, Stream)"]
isTextLike{"ReadTextAsync: IsTextLike(ProjectFile.ContentType)?"}
returnNull["Return null (not text-like)"]
readLoop["Read up to cap bytes in loop until EOF or cap"]
returnString["Return UTF8 string of bytes read"]

step1 --> step2
step2 --> checkFound
checkFound -- yes --> resolvePath
checkFound -- no --> notFound
resolvePath --> checkDisk
checkDisk -- no --> missingOnDisk
checkDisk -- yes --> openStream
openStream --> isTextLike
isTextLike -- no --> returnNull
isTextLike -- yes --> readLoop
readLoop --> returnString
```

```csharp
public sealed class DiskProjectFileService : IProjectFileService
```


Stores and retrieves project files by keeping file contents on local disk and file metadata in the database. Use this service when you want durable, file-system-backed storage for project attachments where files are placed under {Root}/{ProjectId:N}/{filename} and every operation enforces authorization and path-traversal protections before touching disk.

## Remarks
This implementation ties project file metadata (ProjectFile) to physical files on disk and is intended for environments where a shared, mountable filesystem is acceptable. It centralizes concerns that callers would otherwise need to implement themselves: filename sanitization, extension validation, a collision-resilient naming strategy, strict path resolution to prevent traversal attacks, and consistent authorization checks before any read or write. The service logs mismatches between database metadata and on-disk state and surfaces NotFoundException when files are absent.

## Notes
- OpenAsync returns a FileStream that the caller is responsible for disposing; the method comment explicitly delegates disposal to the caller.
- ReadTextAsync only returns text for content types recognized as "text-like"; for non-text content it returns null. It also caps the returned data to the smaller of the provided maxBytes and the file's recorded size and decodes bytes using UTF-8.
- Uploaded filenames are sanitized and validated against AllowedExtensions; when a sanitized name collides the service chooses a short, fresh suffix so concurrent uploads do not clobber each other.
- Before any disk access the service resolves the final path and verifies it is inside the project's directory to mitigate path-traversal attacks; if metadata exists but the on-disk file is missing a warning is logged and NotFoundException is thrown.