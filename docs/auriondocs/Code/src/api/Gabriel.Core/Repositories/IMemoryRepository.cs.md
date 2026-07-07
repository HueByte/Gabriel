# IMemoryRepository

> **File:** `src/api/Gabriel.Core/Repositories/IMemoryRepository.cs`  
> **Kind:** interface

```csharp
public interface IMemoryRepository
```


An abstraction over storage for MemoryEntry objects that exposes scoped CRUD operations. Use this interface when application logic or an agent needs to list, retrieve, create, update or remove memory entries without depending on a particular persistence technology; it encapsulates the notion of user-scope (projectId = null) and optional project-scoped memories.

## Remarks
IMemoryRepository centralizes access rules and scoping for memories: callers supply a userId and an optional projectId to indicate the scope. Implementations are free to map these operations to any backing store (database, in-memory cache, etc.). The FindByNameAsync method assumes the provided name/slug is unique within the specified scope and is intended for the common create-or-update decision the agent performs. Some concrete implementations may use change-tracking semantics where Update and Remove operate on tracked entities and require an explicit commit/save outside this interface—check the actual implementation for persistence guarantees.

## Example
```csharp
// Typical create-or-update flow used by an agent's memory_save tool
var existing = await repo.FindByNameAsync(userId, projectId, slug, ct);
if (existing == null)
{
    var entry = new MemoryEntry { Id = Guid.NewGuid(), UserId = userId, ProjectId = projectId, Name = slug, Value = value };
    await repo.AddAsync(entry, ct);
}
else
{
    existing.Value = value;
    repo.Update(existing);
}

// Note: some implementations may require an additional commit/save step after Update/Add.
```

## Notes
- Pass projectId = null to operate in the user's global scope; passing a projectId limits operations to that project's scope.
- FindByNameAsync treats name as unique within the (userId, projectId) scope — callers use it to decide between create and update.
- AddAsync and the query methods are asynchronous; Update and Remove are synchronous and may rely on change tracking or an external unit-of-work. Verify persistence behavior in the concrete repository implementation.