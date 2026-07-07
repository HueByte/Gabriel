# ProjectsController

> **File:** `src/api/Gabriel.API/Controllers/ProjectsController.cs`  
> **Kind:** class

*Figure: How ProjectsController works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
ProjectsController["ProjectsController: receive HTTP request and route to endpoint"]

ProjectsController-->|"GET /projects ⇒ _projects.ListAsync(ct)"|IProjectService
IProjectService-->|"returns list of Project; map ToResponse(includeFiles: false)"|ProjectResponse

ProjectsController-->|"GET /projects/{id} ⇒ _projects.GetWithFilesAsync(id, ct)"|IProjectService
IProjectService-->|"returns Project; map ToResponse(includeFiles: true)"|ProjectResponse

ProjectsController-->|"POST /projects ⇒ bind CreateProjectRequest from body"|CreateProjectRequest
CreateProjectRequest-->|"call CreateAsync(Name, Description, SystemPrompt, ct)"|IProjectService
IProjectService-->|"returns Project"|Project
Project-->|"ToResponse(includeFiles: false) ⇒ CreatedAtAction(Get)"|ProjectResponse

ProjectsController-->|"PATCH /projects/{id} ⇒ bind UpdateProjectRequest from body"|UpdateProjectRequest
UpdateProjectRequest-->|"GetAsync(id, ct) to fetch Project"|IProjectService
IProjectService-->|"returns Project (current state)"|Project
UpdateProjectRequest-->|"if Name != null ⇒ RenameAsync(id, Name, ct)"|IProjectService
IProjectService-->|"returns Project (renamed)"|Project
UpdateProjectRequest-->|"if Description != null ⇒ UpdateDescriptionAsync(id, Description, ct)"|IProjectService
IProjectService-->|"returns Project (desc updated)"|Project
UpdateProjectRequest-->|"if SystemPrompt != null ⇒ UpdateSystemPromptAsync(id, SystemPrompt, ct)"|IProjectService
IProjectService-->|"returns Project (prompt updated)"|Project
Project-->|"ToResponse(includeFiles: false) ⇒ Ok"|ProjectResponse

ProjectsController-->|"DELETE /projects/{id} ⇒ _projects.DeleteAsync(id, ct)"|IProjectService
IProjectService-->|"deletes project ⇒ NoContent"|ProjectsController

ProjectsController-->|"GET /projects/{id}/sequence ⇒ _sequence.GetForProjectAsync(id, ct)"|IGabrielSequenceService
IGabrielSequenceService-->|"returns sequence"|GabrielSequenceResponse
GabrielSequenceResponse-->|"ToResponse() ⇒ Ok"|ProjectsController
```

```csharp
[ApiController]
[Authorize]
[Route("projects")]
public class ProjectsController : ControllerBase
```


An ASP.NET Core API controller that exposes CRUD operations and project-specific utilities for Project entities (list, get, create, update, delete), plus project-level Gabriel sequence and avatar management endpoints. Use this controller when implementing or calling the HTTP API for managing projects and their shared avatar/sequence state (routes are rooted at /projects and the controller requires authorization).

## Remarks
This controller is a thin HTTP layer that delegates business logic to IProjectService and IGabrielSequenceService. It converts domain Project results into ProjectResponse DTOs for transport and maps HTTP verbs to higher-level operations (Create -> CreateAsync, PATCH -> a series of targeted update calls, etc.). The GetSequence endpoint uses the IGabrielSequenceService aggregation for project-level sequences; callers should use the sequence endpoint for non-default projects while default-project behavior falls back to per-conversation sequences (the aggregation rule lives in IGabrielSequenceService.GetForProjectAsync).

## Notes
- PATCH semantics: the update endpoint treats the request DTO as an all-nullable patch where supplied (non-null) fields are applied, and null is intended to clear values. JSON deserialization cannot distinguish between a missing property and a JSON null, so the current implementation treats both as null — see the project's PATCH design note for explicit-clear behavior.
- Avatar reroll preserves any pinned pattern/palette overrides; reroll only changes seed-derived avatar dimensions.
- Skin pinning/clearing uses PUT semantics (commented in source): each skin field is taken as the full intended state and null clears that dimension; unknown pattern/palette identifiers are rejected with 400 to avoid client-side confusion.
- All controller actions accept a CancellationToken and return standard IActionResult-derived responses (Ok, CreatedAtAction, NoContent), enabling HTTP-friendly status codes and cancellation propagation.