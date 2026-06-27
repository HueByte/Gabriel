# IProjectRepository

> **File:** `src/api/Gabriel.Core/Repositories/IProjectRepository.cs`  
> **Kind:** interface

Repository abstraction for querying and mutating Project entities scoped to a specific owner user. Use this interface when you need to load, list, create, update, delete, or perform owner-scoped bulk operations on projects rather than dealing with data access logic directly.

## Remarks
This interface applies the repository pattern to project storage and enforces owner-scoping via the ownerUserId parameter on read operations and the bulk-assignment operation. Implementations are expected to load related data when requested (GetByIdWithFilesAsync) and to perform efficient bulk updates for operations like AssignOrphanConversationsAsync. CancellationToken parameters should be honored by implementations to allow cooperative cancellation of I/O-bound operations.

## Example
```csharp
// Typical usage in an async service method
var project = new Project { Name = "My Project", OwnerUserId = ownerId };
await projectRepository.AddAsync(project, cancellationToken);

// Later: load project with its files
var loaded = await projectRepository.GetByIdWithFilesAsync(project.Id, ownerId, cancellationToken);
if (loaded != null)
{
    loaded.Name = "Renamed";
    projectRepository.Update(loaded);
}

// Bulk-assign orphan conversations to this project
int changed = await projectRepository.AssignOrphanConversationsAsync(ownerId, project.Id, cancellationToken);
```

## Notes
- Read methods return null when a project is not found; callers should handle null results.
- ownerUserId must be used by implementations to scope queries and prevent cross-user access; failing to do so is a security risk.
- Update and Remove are synchronous (void) on this interface; implementations may require an explicit commit/save step elsewhere or may persist changes immediately — check the concrete implementation's semantics.
- AssignOrphanConversationsAsync returns the number of conversations that were updated as part of the bulk operation.
- CancellationToken parameters should be forwarded to any underlying I/O or database calls to avoid blocking cancellation.