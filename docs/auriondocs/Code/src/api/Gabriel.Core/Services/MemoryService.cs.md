# MemoryService

> **File:** `src/api/Gabriel.Core/Services/MemoryService.cs`  
> **Kind:** class

*Figure: How MemoryService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
  StartSave["MemoryService: SaveAsync(MemoryEntrySpec)"]
  CheckUser["RequireUserId -> ICurrentUser.UserId or throw Unauthorized"]
  FindByName["IMemoryRepository.FindByNameAsync(userId, spec.ProjectId, spec.Name)"]
  D{ "existing is null?" }
  Create["Create MemoryEntry; IMemoryRepository.AddAsync(entry)"]
  Update["Update existing; IMemoryRepository.Update(existing)"]
  Save["IUnitOfWork.SaveChangesAsync()"]
  Return["Return MemoryEntry"]

  StartSave --> CheckUser
  CheckUser --> FindByName
  FindByName --> D
  D -- yes --> Create
  D -- no --> Update
  Create --> Save
  Update --> Save
  Save --> Return
```

```csharp
public class MemoryService : IMemoryService
```


Provides a user-scoped service for managing MemoryEntry objects. MemoryService coordinates an IMemoryRepository and an IUnitOfWork to list, retrieve, create/update (upsert by name within a project), and remove memory entries for the currently authenticated user. Use this when you need application-level operations that enforce the current user's scope and persist changes via the unit-of-work.

## Remarks
This is a thin application service that enforces user scoping and implements simple upsert semantics: SaveAsync will create a new MemoryEntry if none exists with the same name for the current user and project, or update the existing one. It delegates storage details to IMemoryRepository and transaction/commit responsibilities to IUnitOfWork. Authentication is required for all operations — the service obtains the user id from ICurrentUser and throws if it is missing.

## Example
```csharp
// Typical use from a controller or higher-level service
var spec = new MemoryEntrySpec {
    ProjectId = projectId,
    Type = MemoryType.Note,
    Name = "DesignDecision",
    Description = "Why we chose X",
    Body = "Details..."
};

var saved = await memoryService.SaveAsync(spec, cancellationToken);
// saved is the created or updated MemoryEntry for the current user
```

## Notes
- All methods require an authenticated user; if ICurrentUser.UserId is null the service throws UnauthorizedAccessException.
- SaveAsync treats name+project (scoped to the user) as the uniqueness key — callers should expect upsert behavior rather than always receiving a new entity.
- GetByIdAsync and the Remove* methods will return null/false when the targeted entry does not exist or does not belong to the current user; no exception is thrown in those cases.
- Concurrency and detailed persistence semantics (e.g., optimistic concurrency) are delegated to the repository/unit-of-work implementations.