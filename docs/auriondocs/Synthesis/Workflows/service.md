# Adding a new service

> *Workflow template auto-derived from 7 existing exemplar(s).*

When you need to add a new business/service layer that encapsulates domain logic and is consumed by controllers or other services, add a pair of types (an interface and its implementation) alongside the existing services. The repository already follows a small, consistent pattern for these services; model your new service on the existing interfaces and implementations shown below.

## Reference implementation

```csharp
// Service layer over IMemoryRepository. Pulls UserId from ICurrentUser so
// controllers and tools don't pass it everywhere; enforces user-scoping at
// this boundary so a tool can't accidentally cross-read another user's
// memory by manipulating its arguments.
public interface IMemoryService
{
    // All memories the calling user has in the given scope. Pass projectId=null
    // for user-scope only.
    Task<IReadOnlyList<MemoryEntry>> ListAsync(Guid? projectId, CancellationToken ct = default);

    // What the agent should see for a given conversation: user-scope memories
    // plus (if applicable) the conversation's project-scope memories. Returned
    // sorted in display order (Type, then Name).
    Task<IReadOnlyList<MemoryEntry>> ListForConversationAsync(Guid? projectId, CancellationToken ct = default);

    Task<MemoryEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // Upsert: creates a new entry if (UserId, ProjectId, Name) is free, or
    // updates the existing one in place. Returns the saved entity either way.
    // Idempotent — calling twice with the same spec is a no-op apart from
    // bumping UpdatedAt.
    Task<MemoryEntry> SaveAsync(MemoryEntrySpec spec, CancellationToken ct = default);

    // Returns false if no entry matched (vs. true on actual delete) so the
    // memory_remove tool can give the model a clear "wasn't there" response.
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
    Task<bool> RemoveByNameAsync(Guid? projectId, string name, CancellationToken ct = default);
}
```

## Where it lives

Place the service interface and its implementation in the src/api/Gabriel.Core/Services folder. The repository shows interfaces named I{Name}Service (for example, IChatService, IMemoryService, IProjectService) and concrete implementations named {Name}Service (for example, ChatService, MemoryService, ProjectService); mirror that naming and location for your new service.

## Wiring

Detected wiring-site files: src/api/Gabriel.Core/DependencyInjection.cs and src/api/Gabriel.API/Controllers/ConversationsController.cs. Inspect src/api/Gabriel.Core/DependencyInjection.cs to see how existing services are registered, and look at src/api/Gabriel.API/Controllers/ConversationsController.cs for an example of a controller consuming services; use those files as the places to add registration and to verify consumption.

## Existing examples

- [`ChatService`](../../Code/src/api/Gabriel.Core/Services/ChatService.cs.md)
- [`IChatService`](../../Code/src/api/Gabriel.Core/Services/IChatService.cs.md)
- [`IMemoryService`](../../Code/src/api/Gabriel.Core/Services/IMemoryService.cs.md)
- [`IProjectFileService`](../../Code/src/api/Gabriel.Core/Services/IProjectFileService.cs.md)
- [`IProjectService`](../../Code/src/api/Gabriel.Core/Services/IProjectService.cs.md)
- [`MemoryService`](../../Code/src/api/Gabriel.Core/Services/MemoryService.cs.md)
- [`ProjectService`](../../Code/src/api/Gabriel.Core/Services/ProjectService.cs.md)

---
*Synthesised by Aurion on 2026-07-07 21:08:57 UTC*
