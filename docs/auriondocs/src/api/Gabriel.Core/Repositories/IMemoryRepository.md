# IMemoryRepository

> **File:** `src/api/Gabriel.Core/Repositories/IMemoryRepository.cs`  
> **Kind:** interface

An abstraction for storing and querying MemoryEntry items scoped to a user and optionally to a project. Use this repository when higher-level services or agents need to list all memories for a scope, fetch a single entry, look up an entry by its scope-unique name (slug), or persist changes (create/update/remove) without tying those operations to a particular persistence implementation.

## Remarks
This interface isolates storage concerns for conversational memory so callers can work in terms of user- and project-scoped memories without depending on a database or ORM. ListForAgentAsync is provided as a convenience to surface the combined set of memories an agent should "see" for a conversation (user-scope plus the project's scope when applicable). FindByNameAsync uses a scope-unique name to let callers decide whether to create or update an entry.

## Example
```csharp
// List memories visible to an agent in a project-aware conversation
var memories = await memoryRepo.ListForAgentAsync(userId, projectId, ct);

// Create a new memory
var entry = new MemoryEntry { Id = Guid.NewGuid(), UserId = userId, ProjectId = projectId, Name = "todo", Content = "Buy milk" };
await memoryRepo.AddAsync(entry, ct);

// Update an existing memory (implementation may track changes)
entry.Content = "Buy almond milk";
memoryRepo.Update(entry);

// Remove a memory
memoryRepo.Remove(entry);
```

## Notes
- Passing projectId = null to ListAsync or FindByNameAsync targets the user-level scope (no project). 
- FindByNameAsync expects the combination (UserId, ProjectId, Name) to be unique within that scope; callers typically use it to decide between creating or updating a memory.
- Update and Remove are synchronous void methods on this interface; implementations commonly rely on change tracking or an external unit-of-work/commit step. Ensure you understand the concrete repository's lifetime/commit semantics to persist updates.
