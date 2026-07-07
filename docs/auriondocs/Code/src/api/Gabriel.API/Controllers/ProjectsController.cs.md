# ProjectsController

> **File:** `src/api/Gabriel.API/Controllers/ProjectsController.cs`  
> **Kind:** class

*Figure: How ProjectsController works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
ProjectsController["ProjectsController: receive HTTP request"]
CreateProjectRequest["CreateProjectRequest (request body)"]
UpdateProjectRequest["UpdateProjectRequest (patch body)"]
IProjectService["IProjectService (async project operations)"]
IGabrielSequenceService["IGabrielSequenceService (GetForProjectAsync)"]
Project["Project (domain entity)"]
ProjectResponse["ProjectResponse (serialized response)"]
GabrielSequenceResponse["GabrielSequenceResponse (serialized sequence)"]

ProjectsController --|"GET /projects"| IProjectService
IProjectService --|"ListAsync(ct) -> list of Project"| ProjectsController
ProjectsController --|"Map to ProjectResponse (includeFiles: false)"| ProjectResponse

ProjectsController --|"GET /projects/{id}"| IProjectService
IProjectService --|"GetWithFilesAsync(id) -> Project"| Project
Project --|"ToResponse(includeFiles: true)"| ProjectResponse

ProjectsController --|"POST /projects with CreateProjectRequest"| CreateProjectRequest
CreateProjectRequest --|"CreateAsync(name,description,systemPrompt,ct)"| IProjectService
IProjectService --|"returns Project (new)"| Project
Project --|"CreatedAtAction -> ToResponse(includeFiles: false)"| ProjectResponse

ProjectsController --|"PATCH /projects/{id} with UpdateProjectRequest"| UpdateProjectRequest
UpdateProjectRequest --|"GetAsync(id)"| IProjectService
IProjectService --|"returns Project"| Project
UpdateProjectRequest --|"if Name != null -> RenameAsync"| IProjectService
UpdateProjectRequest --|"if Description != null -> UpdateDescriptionAsync"| IProjectService
UpdateProjectRequest --|"if SystemPrompt != null -> UpdateSystemPromptAsync"| IProjectService
Project --|"ToResponse(includeFiles: false)"| ProjectResponse

ProjectsController --|"DELETE /projects/{id}"| IProjectService
IProjectService --|"DeleteAsync(id)"| ProjectsController

ProjectsController --|"GET /projects/{id}/sequence"| IGabrielSequenceService
IGabrielSequenceService --|"GetForProjectAsync(id) -> sequence"| GabrielSequenceResponse
GabrielSequenceResponse --|"ToResponse() -> returned body"| ProjectsController
```

```csharp
[ApiController]
[Authorize]
[Route("projects")]
public class ProjectsController : ControllerBase
```


Provides HTTP endpoints under the "projects" route to manage project life-cycle and project-scoped resources. The controller is an ASP.NET Core ApiController that requires authentication ([Authorize]) and delegates business logic to IProjectService and IGabrielSequenceService. Endpoints include listing projects, retrieving a single project (with files), creating, patch-updating, deleting projects, fetching the project-level Gabriel sequence, and rerolling the project's avatar seed.

## Remarks
This controller is intentionally thin: it performs request/response mapping, route binding, and basic HTTP semantics (e.g. returning Ok, CreatedAtAction, NoContent) while deferring validation and domain operations to the injected services. Responses are produced by converting domain models to ProjectResponse/GabrielSequenceResponse using the domain-to-response helpers (ToResponse). The controller also centralizes the API surface for project-scoped operations so authorization and routing are consistent across project endpoints.

## Example
```csharp
// Create a new project with HttpClient
var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "<token>");

var create = new { Name = "My Project", Description = "Notes", SystemPrompt = "" };
var resp = await client.PostAsJsonAsync("projects", create, cancellationToken);
resp.EnsureSuccessStatusCode();
var created = await resp.Content.ReadFromJsonAsync<ProjectResponse>(cancellationToken: cancellationToken);
Console.WriteLine($"Created project {created.Id} - location: {resp.Headers.Location}");

// Get project-level sequence
var seqResp = await client.GetAsync($"projects/{created.Id}/sequence", cancellationToken);
seqResp.EnsureSuccessStatusCode();
var sequence = await seqResp.Content.ReadFromJsonAsync<GabrielSequenceResponse>(cancellationToken: cancellationToken);
```

## Notes
- PATCH semantics: the Update endpoint uses a nullable DTO and treats any null-valued field as "do not change" vs "explicit clear" inconsistently because JSON deserialization cannot distinguish missing properties from explicit nulls; the controller comments this is a simplification and may change when explicit-clear behavior is introduced. Be careful when attempting to clear Description or SystemPrompt.
- List vs Get: List(...) returns ProjectResponse objects with includeFiles: false (no file list), while Get(...) returns includeFiles: true (includes files). Clients that need file details must call the single-project GET.
- Authentication: the controller requires an authenticated caller ([Authorize]). The controller itself does not implement per-resource permission checks — those are performed by the injected services or other middleware.
- Create returns 201 Created with a Location header pointing to the GET endpoint for the created project (CreatedAtAction).