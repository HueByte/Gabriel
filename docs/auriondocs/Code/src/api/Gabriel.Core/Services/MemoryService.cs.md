# MemoryService

> **File:** `src/api/Gabriel.Core/Services/MemoryService.cs`  
> **Kind:** class

```csharp
public class MemoryService : IMemoryService
```


Implements IMemoryService as an application-layer service that manages MemoryEntry instances for the currently authenticated user. It delegates retrieval and persistence to an IMemoryRepository and uses IUnitOfWork to commit changes; use this service when you need user-scoped listing, fetching, upserting (create-or-update-by-name), or removal of memories.

## Remarks
MemoryService centralizes the enforcement of the authenticated user context (via ICurrentUser) and the higher-level business rule that a memory is unique by (user, project, name). SaveAsync performs an "upsert by name": it looks for an existing entry for the same user and project and either updates that entry or creates a new one, then calls the unit-of-work to persist the change. Read operations are forwarded directly to the repository; ListForConversationAsync delegates to the repository method intended for agent/conversation usage.

## Notes
- If ICurrentUser.UserId is null the service throws UnauthorizedAccessException — callers must ensure an authenticated user is available.
- SaveAsync identifies existing entries by name (and project) and will update an existing record rather than creating a second entry with the same name; be careful of name collisions within the same user/project scope.
- There is no explicit concurrency control in this class: the read-then-write pattern in SaveAsync can produce race conditions if multiple callers try to save the same name concurrently; rely on repository/DB-level constraints or transactions to handle conflicts.