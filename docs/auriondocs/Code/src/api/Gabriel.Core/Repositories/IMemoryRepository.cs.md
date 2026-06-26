# IMemoryRepository

> **File:** `src/api/Gabriel.Core/Repositories/IMemoryRepository.cs`  
> **Kind:** interface

Abstraction for storing and retrieving MemoryEntry objects that are scoped to a user and optionally to a project. Use this repository when implementing persistence for agent or user memories; it exposes listing and lookup operations (including a convenience method that returns all entries an agent should "see" for a conversation), plus methods to add, update, and remove entries.

## Remarks
This interface follows the repository pattern to separate storage concerns from business logic. ListForAgentAsync is a convenience that returns the union of user-scoped memories and, when a projectId is provided, the project-scoped memories so an agent can get everything visible in a single call. FindByNameAsync uses the (userId, projectId, name) tuple because the name (slug) is unique within its scope and is intended to let callers decide between creating a new memory or updating an existing one.

## Example
```csharp
// Typical create-or-update flow used by an agent's memory_save tool
var existing = await repo.FindByNameAsync(userId, projectId, name, ct);
if (existing == null)
{
    var newEntry = new MemoryEntry { Id = Guid.NewGuid(), UserId = userId, ProjectId = projectId, Name = name, Value = value };
    await repo.AddAsync(newEntry, ct);
}
else
{
    existing.Value = value;
    repo.Update(existing); // synchronous update; persistence semantics depend on implementation
}

// Get everything the agent should see for a conversation
var visible = await repo.ListForAgentAsync(userId, projectId, ct);
```

## Notes
- Pass projectId = null to operate in the user scope (project-agnostic memories).
- Name/slug uniqueness is scoped to (userId, projectId); FindByNameAsync is the intended way to detect existing entries.
- AddAsync is asynchronous; Update and Remove are synchronous on the interface — concrete implementations may treat these as tracked changes that require an explicit save/commit step or may persist immediately. Include CancellationToken when calling async methods to support cancellation.
