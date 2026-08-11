# Adding a new service

> *Workflow template auto-derived from 7 existing exemplar(s).*

Adding a new service in this codebase means creating a pair of types — an interface and its implementation — and registering/injecting them where other services live. Reach for this pattern when you need a transactional, testable boundary for business logic (something like the existing ChatService or MemoryService). The exemplars and wiring sites below show the concrete files to mimic and the places to register and consume your new service.

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

Look in src/api/Gabriel.Core/Services for service interfaces and implementations. The exemplars place interface types such as IChatService and IMemoryService alongside concrete classes ChatService and MemoryService in that Services folder; follow the same placement and naming pattern when adding a new service (an interface named I{Name}Service and a class named {Name}Service that implements it).

## Wiring

The repository analysis detected these wiring sites; check them to see how services are registered and consumed and to add your new service where appropriate:

- src/api/Gabriel.Core/DependencyInjection.cs
- src/api/Gabriel.API/Controllers/ConversationsController.cs

## Existing examples

- [`ChatService`](../../Code/src/api/Gabriel.Core/Services/ChatService.cs.md)
- [`IChatService`](../../Code/src/api/Gabriel.Core/Services/IChatService.cs.md)
- [`IMemoryService`](../../Code/src/api/Gabriel.Core/Services/IMemoryService.cs.md)
- [`IProjectFileService`](../../Code/src/api/Gabriel.Core/Services/IProjectFileService.cs.md)
- [`IProjectService`](../../Code/src/api/Gabriel.Core/Services/IProjectService.cs.md)
- [`MemoryService`](../../Code/src/api/Gabriel.Core/Services/MemoryService.cs.md)
- [`ProjectService`](../../Code/src/api/Gabriel.Core/Services/ProjectService.cs.md)

---
*Synthesised by Aurion on 2026-07-08 05:47:19 UTC*
