# ProjectService

> **File:** `src/api/Gabriel.Core/Services/ProjectService.cs`  
> **Kind:** class

*Figure: How ProjectService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
Start["ProjectService (IProjectService) entry"] --> ReqUser["Require user id via ICurrentUser"]
ReqUser --> Op{"Operation type: List | Get | GetWithFiles | Create | Update"}

Op --|List|--> EnsureDefault["Ensure default exists: EnsureDefaultInternalAsync(userId)"]
EnsureDefault --> ListCall["IProjectRepository.ListAsync(userId) -> return list of Project"]
ListCall --> End["End"]

Op --|Get / GetWithFiles|--> GetCall["IProjectRepository.GetByIdAsync / GetByIdWithFilesAsync(id, userId)"]
GetCall --> FoundQ{"Result null?"}
FoundQ -- Yes --> ThrowNotFound["Throw NotFoundException(nameof Project, id)"]
FoundQ -- No --> ReturnGet["Return Project"]
ThrowNotFound --> End
ReturnGet --> End

Op --|Create|--> CreateNode["Project.Create(userId, name, description, systemPrompt) -> Project"]
CreateNode --> AddAsync["IProjectRepository.AddAsync(project)"]
AddAsync --> SaveCreate["IUnitOfWork.SaveChangesAsync()"]
SaveCreate --> ReturnCreate["Return Project"]
ReturnCreate --> End

Op --|"Update (rename/desc/prompt/reroll/skin)"|--> UpdateGet["Call GetAsync(id) to load Project"]
UpdateGet --> Modify["Modify Project via Project.* methods"]
Modify --> UpdateRepo["IProjectRepository.Update(project)"]
UpdateRepo --> SaveUpdate["IUnitOfWork.SaveChangesAsync()"]
SaveUpdate --> ReturnUpdate["Return Project"]
ReturnUpdate --> End
```

```csharp
public class ProjectService : IProjectService
```


Manages user-scoped Project entities: creating, reading, updating and deleting projects for the current user, and ensuring a per-user "Default" project exists. Use this service from application-level code (e.g. controllers or handlers) when you need to perform project operations that must be scoped to the authenticated user and persisted via the repository/unit-of-work.

## Remarks
ProjectService is an application service that enforces per-user scoping and coordinates persistence. It delegates data access to an IProjectRepository and uses an IUnitOfWork to commit changes. Several read and write operations call RequireUserId() to resolve the current user (and implicitly enforce authentication). Before returning a list of projects it ensures a Default project exists for the user so callers always observe at least one project and legacy, project-less conversations can be attached automatically.

## Example
```csharp
// Typical usage inside a controller or handler
var project = await projectService.CreateAsync("My Project", "desc", null, cancellationToken);
project = await projectService.RenameAsync(project.Id, "Renamed", cancellationToken);
var all = await projectService.ListAsync(cancellationToken);
```

## Notes
- All operations are performed for the current authenticated user; RequireUserId() is used internally and will fail if there is no current user.
- GetAsync and GetWithFilesAsync throw NotFoundException when the requested project does not exist or does not belong to the current user.
- Mutating operations call IUnitOfWork.SaveChangesAsync to persist changes; callers can rely on changes being committed when the method completes.
- ListAsync ensures a Default project exists for the user before returning (the implementation is idempotent per the inline comment).