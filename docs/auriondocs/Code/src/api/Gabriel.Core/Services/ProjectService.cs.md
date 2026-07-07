# ProjectService

> **File:** `src/api/Gabriel.Core/Services/ProjectService.cs`  
> **Kind:** class

*Figure: How ProjectService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
A["ProjectService method invoked"]
B["RequireUserId via ICurrentUser"]
D{ "Which method?" }
L["ListAsync: Ensure Default exists (create Project via Project; add via IProjectRepository; save via IUnitOfWork) then IProjectRepository.ListAsync"]
Gt["GetAsync / GetWithFilesAsync: IProjectRepository.GetByIdAsync -> if null throw NotFoundException"]
Cn["CreateAsync: Project.Create -> IProjectRepository.AddAsync -> IUnitOfWork.SaveChangesAsync"]
Up["Rename/Update/Reroll/SetSkin: GetAsync -> mutate Project -> IProjectRepository.Update -> IUnitOfWork.SaveChangesAsync"]
E["Return Project(s) or updated Project"]

A --> B
B --> D
D -- "ListAsync" --> L
D -- "GetAsync / GetWithFilesAsync" --> Gt
D -- "CreateAsync" --> Cn
D -- "Rename/Update/Reroll/SetSkin" --> Up

L --> E
Gt --> E
Cn --> E
Up --> E
```

```csharp
public class ProjectService : IProjectService
```


Manages user-scoped Project entities: lists, retrieves (with or without files), creates, renames, updates fields (description, system prompt, skin, avatar), deletes, and ensures a per-user "Default" project exists. Use this service when you need application-level project operations that require repository access, unit-of-work persistence, and enforcement of the current-user scope rather than working directly with repositories.

## Remarks
ProjectService is the application-layer coordinator for Project operations. It centralizes user-scoping (every operation requires the current user), ensures the presence of a single per-user Default project (so legacy, project-less conversations can be back-filled), and coordinates repository changes with IUnitOfWork.SaveChangesAsync. By encapsulating these responsibilities it keeps calling code free of repository and transaction concerns and provides consistent NotFound/authorization behavior across project operations.

## Example
```csharp
// Typical usage from a controller or higher-level service where IProjectService
// has been injected via dependency injection.
public class ProjectsController
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects)
    {
        _projects = projects;
    }

    public async Task ListAndCreateExample()
    {
        // Ensure the user has a default project and get the list
        var all = await _projects.ListAsync();

        // Create a new project for the current user
        var created = await _projects.CreateAsync("My Project", "A description", null);

        // Rename the newly created project
        var renamed = await _projects.RenameAsync(created.Id, "Renamed Project");

        // Retrieve the default project's id (idempotent)
        var defaultId = await _projects.EnsureDefaultProjectIdAsync();
    }
}
```

## Notes
- All operations are scoped to the current user (RequireUserId is used internally); calling code should expect UnauthorizedAccessException when there is no authenticated user.
- Get/GetWithFiles and other read operations throw NotFoundException if the requested project does not exist or is not owned by the current user.
- Mutating methods call IUnitOfWork.SaveChangesAsync; callers should be prepared to handle persistence-related exceptions (e.g., concurrency or DB errors) as appropriate.
- EnsureDefaultProjectIdAsync is idempotent: it returns the existing Default project's id or creates and returns one on the first call for a user; subsequent calls are cheap.