# ProjectFilesController

> **File:** `src/api/Gabriel.API/Controllers/ProjectFilesController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("projects/{projectId:guid}/files")]
public class ProjectFilesController : ControllerBase
```


ProjectFilesController exposes a RESTful API surface for managing files associated with a specific project. It provides endpoints to list all files for a project, download an individual file, upload a new file via multipart/form-data (single file), and delete a file. The controller delegates storage and retrieval to IProjectFileService and is secured with authorization.

## Remarks
The controller concentrates HTTP concerns (routing, model binding, response codes) and defers business logic to the service, keeping concerns separated. It streams file downloads via FileStreamResult and uses CreatedAtAction to supply a link to the uploaded file, emphasizing a discoverable workflow for clients. IFormFile binding is automatic under ApiController, so no FromForm attribute is required, which keeps runtime binding and OpenAPI generation in sync.

## Notes
- The Upload action enforces a 50 MB limit via RequestSizeLimit; adjust as needed for larger files or different environments.
- The FileStreamResult ensures the underlying stream is disposed after the response. The code uses await using to dispose the input stream promptly; do not wrap the stream disposal manually.
- Ensure IProjectFileService is registered in the DI container and implements the expected operations (ListAsync, OpenAsync, UploadAsync, DeleteAsync) for this controller to function correctly.