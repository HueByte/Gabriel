# DiskProjectFileService

> **File:** `src/api/Gabriel.Infrastructure/Projects/DiskProjectFileService.cs`  
> **Kind:** class

*Figure: How DiskProjectFileService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
start["Start: DiskProjectFileService.ReadTextAsync"]
auth["Authorize via IProjectRepository and ICurrentUser"]
dbQuery["AppDbContext: query ProjectFiles for ProjectFile by fileId & projectId"]
found{"ProjectFile exists?"}
notFound["Throw NotFoundException(nameof(ProjectFile), fileId)"]
getFile["Got ProjectFile metadata (ProjectFile)"]
resolve["Resolve file path using ProjectFilesOptions.Root; verify path is inside project dir"]
existsDisk{"Disk file exists?"}
warnNotFound["Log warning; Throw NotFoundException(ProjectFile,fileId)"]
open["Open FileStream (FileMode.Open, FileAccess.Read, FileShare.Read)"]
isText{"IsTextLike(ProjectFile.ContentType)?"}
readLoop["Read up to min(maxBytes, file.SizeBytes) in loop from stream"]
returnText["Return UTF8 string"]
returnNull["Return null (not text-like)"]
done["Done"]

start --> auth
auth --> dbQuery
dbQuery --> found
found -- "No" --> notFound
notFound --> done
found -- "Yes" --> getFile
getFile --> resolve
resolve --> existsDisk
existsDisk -- "No" --> warnNotFound
warnNotFound --> done
existsDisk -- "Yes" --> open
open --> isText
isText -- "No" --> returnNull
returnNull --> done
isText -- "Yes" --> readLoop
readLoop --> returnText
returnText --> done
```

```csharp
public sealed class DiskProjectFileService : IProjectFileService
```


Stores and serves project file content on the local filesystem while keeping file metadata in the application's database. Use this implementation when you want a simple, predictable on-disk layout for project files (root/{ProjectId:N}/{filename}) and prefer local disk storage instead of a remote blob service. All operations perform authorization checks and validate filesystem paths to prevent path-traversal.

## Remarks
This class separates file content (on-disk) from metadata (ProjectFiles table in the DbContext). It enforces a number of safety and consistency rules that are easy to miss when implementing custom file services: filenames are sanitized and restricted by allowed extensions, every resolved path is checked to ensure it lives inside the project's directory (mitigating path-traversal), and the upload path selection uses a suffix-collision policy so concurrent uploads result in distinct final filenames.

## Example
```csharp
// List files
var files = await diskService.ListAsync(projectId, ct);

// Open a file stream for reading (caller must dispose)
var (meta, stream) = await diskService.OpenAsync(projectId, fileId, ct);
using (stream)
{
    // read from stream
}

// Read small text content, null if not a text-like content type
string? text = await diskService.ReadTextAsync(projectId, fileId, maxBytes: 16_384, ct);

// Upload a file stream
using var uploadStream = File.OpenRead("localfile.bin");
var newFile = await diskService.UploadAsync(projectId, "report.txt", "text/plain", uploadStream, ct);
```

## Notes
- OpenAsync returns an open FileStream; the caller is responsible for disposing it to avoid file handles leaking.
- ReadTextAsync returns null if the file's content type is not considered "text-like"; it also caps the read to the provided maxBytes and decodes bytes as UTF-8.
- If metadata exists but the underlying disk file is missing, OpenAsync logs a warning and throws NotFoundException.
- UploadAsync sanitizes filenames and enforces allowed extensions; its collision policy appends a short suffix so concurrent uploads to the same sanitized name do not clobber each other.
- All public operations perform an authorization check (via AuthorizeAsync) before accessing metadata or disk; callers must ensure the current principal has the required project permissions.