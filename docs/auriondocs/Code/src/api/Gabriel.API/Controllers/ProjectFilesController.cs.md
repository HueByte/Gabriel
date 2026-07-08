# ProjectFilesController

> **File:** `src/api/Gabriel.API/Controllers/ProjectFilesController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("projects/{projectId:guid}/files")]
public class ProjectFilesController : ControllerBase
```


ProjectFilesController exposes a RESTful HTTP surface for managing files tied to a specific project. It supports listing all files for a project, downloading a chosen file as a streamed response with the correct content type and filename, uploading a single file via multipart/form-data, and deleting a file. The controller delegates all storage and business logic to IProjectFileService, focusing on HTTP-level concerns, request validation, and response shaping.

The routes are defined under /projects/{projectId:guid}/files, with:
- GET /projects/{projectId}/files to list files
- GET /projects/{projectId}/files/{fileId}/download to download a file
- POST /projects/{projectId}/files to upload a single file (multipart/form-data)
- DELETE /projects/{projectId}/files/{fileId} to delete a file

The implementation emphasizes streaming and proper HTTP semantics: file content is streamed via FileStreamResult with the original ContentType and FileDownloadName preserved, uploads are validated to reject empty files, and the response for a successful upload includes a Location header pointing to the Download action.

## Remarks
ProjectFilesController is a thin HTTP facade over IProjectFileService. This separation keeps HTTP concerns (routing, binding, validation, response shaping) decoupled from storage and business rules, making it easy to swap storage implementations or test the controller in isolation. Streaming via FileStreamResult avoids loading entire files into memory, which is important for large assets. The [Authorize] attribute ensures only authorized users with access to the project may manage files, and the explicit route structure clarifies the intended usage patterns for clients.

The Upload action uses a single-file upload design with implicit form binding for IFormFile (no [FromForm] attribute) to align runtime binding with OpenAPI generation expectations while keeping the runtime path straightforward. A deliberate belt-and-suspenders limit is applied to the request size to prevent excessively large payloads from being accepted in one go.

## Notes
- This controller currently supports a single-file upload per request; batch uploads are not implemented yet.
- Uploads are capped at 50 MB to protect the API from oversized requests; clients should respect this limit or adjust server configuration if higher limits are required.
- The code relies on the framework’s disposal semantics for streams: the FileStreamResult will dispose the underlying stream after the response is written.

## Symbol To Document
- Name: `ProjectFilesController`
- Kind: class
- File: `src/api/Gabriel.API/Controllers/ProjectFilesController.cs`
- Language: csharp
- ID: b95b9682-0f27-45d4-b6d3-3b6eb29a9442