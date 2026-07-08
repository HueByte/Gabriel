# ProjectService

> **File:** `src/api/Gabriel.Core/Services/ProjectService.cs`  
> **Kind:** class

*Figure: How ProjectService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
ProjectService["ProjectService - entry for ListAsync, GetAsync, GetWithFilesAsync, CreateAsync, RenameAsync, UpdateDescriptionAsync, UpdateSystemPromptAsync, RerollAvatarAsync, SetSkinAsync"]
IProjectService["IProjectService - interface implemented by ProjectService"]
ICurrentUser["ICurrentUser - provides current user id via RequireUserId"]
IProjectRepository["IProjectRepository - repository methods: GetByIdAsync, GetByIdWithFilesAsync, ListAsync, AddAsync, Update"]
IUnitOfWork["IUnitOfWork - SaveChangesAsync invoked after mutations"]
Project["Project - factory Create and mutating methods Rename, UpdateDescription, UpdateSystemPrompt, RerollAvatar, SetSkin"]
NotFoundException["NotFoundException - thrown when repository returns null for Get operations"]

IProjectService --> ProjectService
ProjectService --|"RequireUserId used before operations"| ICurrentUser
ProjectService --|"Ensure default exists then call ListAsync"| IProjectRepository
ProjectService --|"Call GetByIdAsync or GetByIdWithFilesAsync"| IProjectRepository
IProjectRepository --|"returns null"| NotFoundException
IProjectRepository --|"returns project entity"| Project
ProjectService --|"On create or update, construct or mutate Project and call repository"| Project
ProjectService --|"AddAsync or Update then call SaveChangesAsync on unit of work"| IUnitOfWork
```

```csharp
public class ProjectService : IProjectService
```


Provides high-level project management operations for the current user and coordinates repository and unit-of-work persistence. Use this service from application layers when you need to list, create, rename, update, or delete Projects for the authenticated user; it wraps domain operations on Project and ensures changes are saved through the IUnitOfWork.

## Remarks
ProjectService enforces per-user scoping (operations act on the currently authenticated user) and guarantees a "Default" project exists for a user before returning a project list. Mutating operations (create, rename, update description/system prompt, reroll avatar, set skin, delete) apply domain logic on the Project entity, update the repository, and call the unit-of-work to persist changes. Read operations throw NotFoundException when the requested project does not exist for the current user.

## Example
```csharp
// 'projectService' is an instance of ProjectService (IProjectService).
// Ensure the default project exists and get its id.
var defaultId = await projectService.EnsureDefaultProjectIdAsync(ct);

// List projects for the current user (this will create the Default project if needed).
var projects = await projectService.ListAsync(ct);

// Create a new project and then rename it.
var newProject = await projectService.CreateAsync("My Project", "notes", null, ct);
var renamed = await projectService.RenameAsync(newProject.Id, "Renamed Project", ct);

// Handle missing project when trying to get it by id.
try
{
    var project = await projectService.GetAsync(Guid.NewGuid(), ct);
}
catch (NotFoundException ex)
{
    // project not found for current user
}
```

## Notes
- All methods operate on the currently authenticated user; calls will fail with an UnauthorizedAccessException if there is no current user context.
- Create/modify/delete methods persist changes by calling IUnitOfWork.SaveChangesAsync — the effects are durable only after the returned task completes.
- GetAsync and GetWithFilesAsync throw NotFoundException when the project id does not exist for the current user.