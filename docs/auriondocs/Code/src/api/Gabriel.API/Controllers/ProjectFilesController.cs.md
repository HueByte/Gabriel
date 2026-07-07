# ProjectFilesController

> **File:** `src/api/Gabriel.API/Controllers/ProjectFilesController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("projects/{projectId:guid}/files")]
public class ProjectFilesController : ControllerBase
```


ProjectFilesController exposes a RESTful API surface to list, download, upload, and delete files tied to a specific project. It delegates the actual storage operations to IProjectFileService and maps results to ProjectFileResponse, returning appropriate HTTP statuses and streaming content for downloads.

## Remarks
ProjectFilesController serves as the API boundary between HTTP clients and the underlying file-domain logic. It is secured with ApiController and Authorize, and its routes are scoped under the project to ensure all file actions occur within the correct project context. Downloads use FileStreamResult to stream content and set the download name from metadata; uploads perform a single-file multipart upload with a pragmatic size limit and return a CreatedAtAction pointing to the downloadable endpoint. The IFormFile parameter for Upload is bound automatically by the framework due to ApiController, so avoid adding an explicit FromForm attribute as Swashbuckle/OpenAPI generation expects; this preserves both runtime binding and OpenAPI contract.

## Notes
- Using Upload binds a non-empty IFormFile named `file` via multipart/form-data; the endpoint will respond with 400 Bad Request if the file is missing or empty and 201 Created for successful uploads.
- The POST Upload endpoint applies a 50 MB request-size cap as a defensive boundary against oversized payloads.
- For downloads, the underlying content stream is disposed by FileStreamResult after the response is written; do not dispose it manually in the controller.