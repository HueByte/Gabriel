# ProjectFilesController

> **File:** `src/api/Gabriel.API/Controllers/ProjectFilesController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("projects/{projectId:guid}/files")]
public class ProjectFilesController : ControllerBase
```


ProjectFilesController is an authenticated API controller exposing REST endpoints at projects/{projectId:guid}/files to list project files, download a file stream, upload a single multipart/form-data file, and delete a file. It delegates operations to an injected IProjectFileService, returns ProjectFileResponse objects for list and upload, streams file content with FileStreamResult for downloads, and returns NoContent after deletes. The Upload action accepts a single IFormFile (form key "file"), enforces a 50 MB request-size limit, validates non-empty input, and intentionally omits [FromForm] on the IFormFile to avoid OpenAPI generation issues.

## Remarks
The controller is a thin HTTP surface: it handles request/response shape, basic validation, and routing, while all business/storage behavior is delegated to IProjectFileService; it also uses CreatedAtAction to link upload responses to the download endpoint and relies on FileStreamResult so the framework disposes the response stream.