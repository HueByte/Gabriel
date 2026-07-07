# IMemoryRepository

> **File:** `src/api/Gabriel.Core/Repositories/IMemoryRepository.cs`  
> **Kind:** interface

```csharp
public interface IMemoryRepository
```


An abstraction for storing and retrieving MemoryEntry objects scoped to a user and optionally to a project. Use this interface when you need a persistence-agnostic way to list, find, add, update, or remove memory entries (for example when implementing different database backends or an in-memory test double), and when you want the agent-facing convenience method that returns the entries the agent should "see" for a conversation.

## Remarks
IMemoryRepository defines the minimal operations the rest of the application requires to manage memory entries: listing a scope, an agent-focused combined list, lookup by id or by the scope-unique name, and basic create/update/delete operations. The interface intentionally separates AddAsync (asynchronous persistence action) from Update (synchronous mutation/marking) to allow implementations to decide how change-tracking and durable flushes are handled (for example an ORM-backed repository may mark entities as modified and persist on a unit-of-work commit).

## Notes
- projectId == null represents the user-level scope; ListAsync with projectId null returns only user-scoped entries.
- Name (the slug) is expected to be unique within the (UserId, ProjectId) scope — FindByNameAsync is used by callers (such as the agent) to decide between creating a new MemoryEntry or updating an existing one.
- Update is a synchronous void method; depending on the repository implementation it may only mark an entity as changed (requiring an additional save/commit step) rather than immediately persisting to storage.