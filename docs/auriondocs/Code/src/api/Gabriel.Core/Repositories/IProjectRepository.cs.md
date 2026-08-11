# IProjectRepository

> **File:** `src/api/Gabriel.Core/Repositories/IProjectRepository.cs`  
> **Kind:** interface

```csharp
public interface IProjectRepository
```


IProjectRepository defines the data-access contract for Project aggregates. All operations are scoped by owner to enforce per-user data isolation, and most methods are asynchronous to reflect I/O-bound persistence. The repository exposes retrieval methods to fetch by id (with or without related files), list all projects for an owner, and locate a first project by name, alongside mutation methods to add, update, or remove projects, and a bulk operation to reassign orphan conversations to a specified project. This abstraction lets domain logic interact with Project data without depending on a particular storage implementation.

## Remarks
Repository abstractions like this separate domain logic from storage concerns and enforce per-owner data boundaries, supporting multi-tenant scenarios. The GetByIdWithFilesAsync variant demonstrates the ability to eagerly load related data when needed, while the AssignOrphanConversationsAsync method coordinates a cross-entity operation to ensure conversations are properly associated with a project during onboarding or maintenance.

## Example
```csharp
// Get a project for an owner
var project = await repository.GetByIdAsync(projectId, ownerId, ct);

// List all projects for an owner
var all = await repository.ListAsync(ownerId, ct);

// Reassign orphan conversations to a specific project
int updated = await repository.AssignOrphanConversationsAsync(ownerId, projectId, ct);
```

## Notes
- Update(Project) and Remove(Project) do not accept a CancellationToken, so cancellation must be handled by the implementation or surrounding unit-of-work/pipeline.
- GetByIdAsync and GetByIdWithFilesAsync differ in how much related data is loaded; choose based on whether you need the associated Files collection.
- All read operations require ownerUserId to maintain proper data isolation; passing the correct ownerId is essential to avoid cross-tenant access.