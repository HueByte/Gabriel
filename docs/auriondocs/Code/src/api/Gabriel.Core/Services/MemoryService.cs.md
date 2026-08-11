# MemoryService

> **File:** `src/api/Gabriel.Core/Services/MemoryService.cs`  
> **Kind:** class

*Figure: How MemoryService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
IMemoryService["IMemoryService: public API (List/Get/Save/Remove)"]
MemoryService["MemoryService: method invoked"]
ICurrentUser["ICurrentUser: UserId or null"]
IMemoryRepository["IMemoryRepository: List/Find/Get/Add/Update/Remove"]
MemoryEntrySpec["MemoryEntrySpec: input for SaveAsync"]
MemoryEntry["MemoryEntry: Create or Update entity"]
IUnitOfWork["IUnitOfWork: SaveChangesAsync (commit)"]

IMemoryService --> MemoryService
MemoryService -->|"RequireUserId()"| ICurrentUser
MemoryService -->|"calls repository"| IMemoryRepository
MemoryService -->|"SaveAsync(spec)"| MemoryEntrySpec

IMemoryRepository -->|"FindByNameAsync(userId, projectId, name)"| MemoryEntry
IMemoryRepository -->|"existing == null: AddAsync(new MemoryEntry)"| MemoryEntry
IMemoryRepository -->|"existing != null: Update(existing)"| MemoryEntry

MemoryService -->|"GetByIdAsync(id, userId) / FindByNameAsync(userId, projectId, name)"| IMemoryRepository

MemoryEntry -->|"added or updated"| IUnitOfWork
MemoryService -->|"SaveChangesAsync()"| IUnitOfWork
IUnitOfWork -->|"commit/return result"| IMemoryService
```

```csharp
public class MemoryService : IMemoryService
```


A service that implements IMemoryService by delegating memory operations to a repository and committing changes through a unit-of-work. Use MemoryService when you need an application-layer façade that enforces the current authenticated user, performs simple upsert logic for memories (by name + project), and coordinates persistence (add/update/remove) with transaction semantics.

## Remarks
MemoryService centralizes user-scoped memory operations: it requires an authenticated user (via ICurrentUser) and then forwards list, retrieve, save, and delete operations to an underlying IMemoryRepository. SaveAsync implements an upsert by searching for an existing memory with the same name and project for the current user — creating a new MemoryEntry when none exists or calling Update on the existing entry otherwise — and always calls IUnitOfWork.SaveChangesAsync to persist modifications.

## Notes
- All operations require an authenticated user; if ICurrentUser.UserId is null the service throws UnauthorizedAccessException.
- SaveAsync matches existing entries by (userId, projectId, name) and will update the first match rather than creating a duplicate.
- RemoveAsync and RemoveByNameAsync return false when the targeted memory cannot be found; they return true only after the repository removal and unit-of-work save complete.
- ListForConversationAsync delegates to the repository's agent-specific listing method (ListForAgentAsync) — use ListAsync for the standard listing behavior and ListForConversationAsync when data tailored for agent/conversation use is needed.