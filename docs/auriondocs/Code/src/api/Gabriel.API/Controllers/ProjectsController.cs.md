# ProjectsController

> **File:** `src/api/Gabriel.API/Controllers/ProjectsController.cs`  
> **Kind:** class

*Figure: How ProjectsController works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
ProjectsController["ProjectsController receives HTTP request"]
ProjectsController --> route{ "Route: HTTP method & path" }

route -->| "GET /projects" | listCall["Call IProjectService.ListAsync(ct)"]
listCall --> mapList["Map to ProjectResponse list (ToResponse includeFiles: false)"]
mapList --> returnList["Return Ok(ProjectResponse list)"]

route -->| "GET /{id}" | getCall["Call IProjectService.GetWithFilesAsync(id, ct)"]
getCall --> mapGet["Map to ProjectResponse (ToResponse includeFiles: true)"]
mapGet --> returnGet["Return Ok(ProjectResponse)"]

route -->| "POST /projects" | postReq["Bind CreateProjectRequest body"]
postReq --> createCall["Call IProjectService.CreateAsync(request.Name, request.Description, request.SystemPrompt, ct)"]
createCall --> created["Return CreatedAtAction(Get, id, ProjectResponse includeFiles: false)"]

route -->| "PATCH /{id}" | patchReq["Bind UpdateProjectRequest body"]
patchReq --> getForPatch["Call IProjectService.GetAsync(id, ct)"]
getForPatch --> checkName{ "Is request.Name not null?" }
checkName -- yes --> renameCall["Call IProjectService.RenameAsync(id, request.Name, ct)"]
checkName -- no --> skipRename["Skip rename"]
renameCall --> continue1["Continue"]
skipRename --> continue1
continue1 --> checkDesc{ "Is request.Description not null?" }
checkDesc -- yes --> descCall["Call IProjectService.UpdateDescriptionAsync(id, request.Description, ct)"]
checkDesc -- no --> skipDesc["Skip description update"]
descCall --> continue2["Continue"]
skipDesc --> continue2
continue2 --> checkPrompt{ "Is request.SystemPrompt not null?" }
checkPrompt -- yes --> promptCall["Call IProjectService.UpdateSystemPromptAsync(id, request.SystemPrompt, ct)"]
checkPrompt -- no --> skipPrompt["Skip system prompt update"]
promptCall --> finalizePatch["Map to ProjectResponse (ToResponse includeFiles: false)"]
skipPrompt --> finalizePatch
finalizePatch --> returnPatch["Return Ok(ProjectResponse)"]

route -->| "DELETE /{id}" | deleteCall["Call IProjectService.DeleteAsync(id, ct)"]
deleteCall --> returnDelete["Return NoContent()"]

route -->| "GET /{id}/sequence" | seqCall["Call IGabrielSequenceService.GetForProjectAsync(id, ct)"]
seqCall --> seqMap["Map to GabrielSequenceResponse (ToResponse)"]
seqMap --> returnSeq["Return Ok(GabrielSequenceResponse)"]
```

```csharp
[ApiController]
[Authorize]
[Route("projects")]
public class ProjectsController : ControllerBase
```


An API controller that exposes HTTP endpoints for managing Project entities and their shared Gabriel sequence. Routes are rooted at "projects" and all actions require an authenticated user; the controller delegates business logic to IProjectService and sequence aggregation to IGabrielSequenceService, returning ProjectResponse and GabrielSequenceResponse DTOs for HTTP clients.

## Remarks
This controller is a thin HTTP surface over the domain services: IProjectService handles create/read/update/delete and avatar operations, while IGabrielSequenceService aggregates the project-level Gabriel sequence used by clients. The controller converts domain Project objects into ProjectResponse DTOs (via ToResponse calls) and uses standard RESTful semantics (GET for list/detail, POST for create returning CreatedAtAction, PATCH for partial updates, DELETE for removal). Project-level sequence and avatar endpoints are provided so clients can obtain the sequence for non-default projects and request avatar rerolls without manipulating project internals.

## Notes
- PATCH semantics: the Update action treats the incoming DTO as an "all-nullable" patch. A null value for Description or SystemPrompt is interpreted as an explicit clear, and a null Name means "leave unchanged". Because JSON deserialization maps both missing properties and explicit nulls to null, the controller currently does not distinguish between omitted fields and explicit clears — this is a known simplification noted in the source.
- Cancellation: every public action accepts a CancellationToken and forwards it to the underlying services; callers can cancel long-running operations.
- Created response: Create returns CreatedAtAction(nameof(Get), new { id = project.Id }, ...) so clients receive a Location header pointing to the project's GET endpoint.
- Authorization: the [Authorize] attribute on the controller applies to all endpoints; ensure callers present appropriate credentials/claims before invoking these routes.
- Avatar behavior: RerollAvatar changes the seed-derived aspects of the avatar while preserving any pattern/palette overrides (if present).