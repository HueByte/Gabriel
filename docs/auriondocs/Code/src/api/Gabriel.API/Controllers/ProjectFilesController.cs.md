# ProjectFilesController

> **File:** `src/api/Gabriel.API/Controllers/ProjectFilesController.cs`  
> **Kind:** class

Exposes authenticated HTTP endpoints for managing files attached to a project. Use this controller when you need a simple REST surface for listing, downloading, uploading (single multipart file), and deleting files that belong to a project; the controller delegates all storage and content handling to an IProjectFileService implementation.

## Remarks
The controller is a thin HTTP adapter: it handles routing, request/response shapes, basic validation, and authentication, and forwards actual file operations to IProjectFileService. Routes are rooted at "projects/{projectId:guid}/files" and each action accepts a CancellationToken which is passed through to the service methods to allow cooperative cancellation. Authorization is enforced at the controller level via [Authorize], so callers must be authenticated according to the application's authentication scheme.

## Example
```csharp
// Upload a file with HttpClient
using var client = new HttpClient();
using var content = new MultipartFormDataContent();
using var fileStream = File.OpenRead("path/to/file.png");
content.Add(new StreamContent(fileStream), "file", "file.png");
var response = await client.PostAsync($"https://api.example.com/projects/{projectId}/files", content);
response.EnsureSuccessStatusCode();

// Download a file with HttpClient
var download = await client.GetAsync($"https://api.example.com/projects/{projectId}/files/{fileId}/download");
download.EnsureSuccessStatusCode();
using var outStream = await download.Content.ReadAsStreamAsync();
// save outStream to disk
```

## Notes
- The Upload action intentionally does not use an explicit [FromForm] on the IFormFile parameter because [ApiController] already infers form binding and adding [FromForm] can break OpenAPI generation (Swashbuckle).
- FileStreamResult disposes the response stream after the response is written, so the controller does not wrap the returned stream in a using/await using; the service should provide an opened stream that remains valid for the response lifetime.
- RequestSizeLimit attribute caps the request body to 50 MB here, but server/Kestrel limits and reverse-proxy settings can also affect allowable upload sizes; this attribute is a belt-and-suspenders measure on the controller level.
- Upload validates that the provided IFormFile is not null and not empty and returns 400 with a short error detail if validation fails.
