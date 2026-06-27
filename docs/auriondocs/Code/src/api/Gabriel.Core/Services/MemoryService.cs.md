# MemoryService

> **File:** `src/api/Gabriel.Core/Services/MemoryService.cs`  
> **Kind:** class

Provides CRUD-style operations for MemoryEntry objects scoped to the currently authenticated user. Use this service from controllers or higher-level application services when you need to list, retrieve, create/update (by name), or delete user-specific memories; persistence is delegated to IMemoryRepository and committed via IUnitOfWork.

## Remarks
MemoryService is an application-level facade that enforces per-user scoping by reading the current user's ID from ICurrentUser and throwing UnauthorizedAccessException if no authenticated user is present. It delegates data access to IMemoryRepository and uses IUnitOfWork to persist changes. SaveAsync treats a memory's (userId, projectId, name) tuple as the uniqueness key: if a memory with that key exists it is updated, otherwise a new MemoryEntry is created and added.

## Example
```csharp
// create or update a memory for the current user
var spec = new MemoryEntrySpec
{
    ProjectId = someProjectId,
    Type = MemoryType.Custom,
    Name = "favorite-snack",
    Description = "User's favorite snack",
    Body = "Chocolate"
};

try
{
    var saved = await memoryService.SaveAsync(spec, CancellationToken.None);
    // saved is the created or updated MemoryEntry
}
catch (UnauthorizedAccessException)
{
    // handle missing authentication
}

// list memories for a project
var list = await memoryService.ListAsync(someProjectId);

// remove by name
var removed = await memoryService.RemoveByNameAsync(someProjectId, "favorite-snack");
```

## Notes
- RequireUserId will throw UnauthorizedAccessException when there is no authenticated user; callers should handle or let it propagate to authentication middleware.
- SaveAsync determines whether to add or update by calling FindByNameAsync with (userId, projectId, name); uniqueness is scoped to the combination of user and project.
- Repository methods (_memories) only stage changes; SaveAsync and the remove methods call IUnitOfWork.SaveChangesAsync to persist them.
- GetByIdAsync returns null when a memory with the given id (and of the current user) is not found; Remove*/RemoveByNameAsync return false in that case.
- All public methods accept a CancellationToken which is forwarded to the repository/uow calls.