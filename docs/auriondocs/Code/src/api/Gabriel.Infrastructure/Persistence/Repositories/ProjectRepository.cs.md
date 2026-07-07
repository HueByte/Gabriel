# ProjectRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/ProjectRepository.cs`  
> **Kind:** class

```csharp
public class ProjectRepository : IProjectRepository
```


Implements IProjectRepository using an EF Core AppDbContext to query and mutate Project entities. Use this repository when you need application-level data access for projects (queries, add/update/remove) and when you need to bulk-assign orphaned conversations to a project without loading those rows into the change tracker.

## Remarks
This is a thin persistence adapter that centralizes Project-related queries and mutations so callers don't need to embed EF Core logic throughout the codebase. Query methods return tracked entities from the provided AppDbContext; mutating methods (AddAsync/Update/Remove) only modify the DbContext state and rely on the caller's unit-of-work to persist changes. The AssignOrphanConversationsAsync method uses EF Core's ExecuteUpdateAsync to perform a server-side, non-tracked bulk update for performance and to avoid loading many Conversation entities.

## Example
```csharp
// Typical use inside a scoped operation that owns the AppDbContext lifetime
var repo = new ProjectRepository(dbContext);
var project = new Project { /* set required properties */ };
await repo.AddAsync(project, ct);
// Persist the added project
await dbContext.SaveChangesAsync(ct);

// Bulk-assign orphan conversations to the new project (this runs immediately in the DB)
int updated = await repo.AssignOrphanConversationsAsync(ownerUserId, project.Id, ct);
// updated is the number of rows affected; no additional SaveChangesAsync is required for this call
```

## Notes
- AddAsync, Update, and Remove only change DbContext state; the caller must call SaveChangesAsync/SaveChanges to persist those changes. AssignOrphanConversationsAsync executes a direct database update and returns the count of affected rows immediately.
- AppDbContext (and therefore this repository) is not thread-safe. Do not use the same repository/DbContext instance concurrently from multiple threads.
- GetByIdWithFilesAsync eagerly includes the project's Files collection and applies an ordering (UploadedAt descending) to that collection so returned Project.Files will be materialized in that order.