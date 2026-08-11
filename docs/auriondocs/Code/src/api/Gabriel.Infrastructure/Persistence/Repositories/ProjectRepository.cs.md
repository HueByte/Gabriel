# ProjectRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/ProjectRepository.cs`  
> **Kind:** class

```csharp
public class ProjectRepository : IProjectRepository
```


Provides CRUD-style access to Project entities backed by an AppDbContext and implements IProjectRepository. Use this repository when you need to query, add, update or remove projects scoped to an owner user, or to bulk-assign orphaned conversations to a project without loading rows into the change tracker.

## Remarks
This class is a thin EF Core-backed repository that centralizes common Project operations and enforces scoping by OwnerUserId on all queries. It intentionally does not call SaveChanges/SaveChangesAsync: persistence is expected to be handled by the surrounding unit-of-work or higher-level service. The AssignOrphanConversationsAsync method uses EF Core's ExecuteUpdateAsync to perform a server-side, non-tracked bulk update (returns number of rows affected), which avoids loading Conversation entities into the change tracker for performance.

## Example
```csharp
// list projects for a user
IProjectRepository repo = new ProjectRepository(dbContext);
var projects = await repo.ListAsync(ownerUserId, cancellationToken);

// add a new project (note: caller must persist changes via the DbContext / unit-of-work)
var newProject = new Project
{
    Id = Guid.NewGuid(),
    OwnerUserId = ownerUserId,
    Name = "New Project",
    CreatedAt = DateTimeOffset.UtcNow,
    UpdatedAt = DateTimeOffset.UtcNow
};
await repo.AddAsync(newProject, cancellationToken);

// bulk assign orphan conversations to a project (returns number of rows updated)
int updated = await repo.AssignOrphanConversationsAsync(ownerUserId, newProject.Id, cancellationToken);
```

## Notes
- None of the modifying methods (AddAsync, Update, Remove) call SaveChanges; callers must save changes on the AppDbContext or via a unit-of-work.
- GetByIdWithFiles includes the project's Files collection and requests them ordered descending by UploadedAt — the order is applied in the Include expression.
- AssignOrphanConversationsAsync performs a server-side bulk update and returns the number of affected rows; because it uses ExecuteUpdateAsync the updated Conversation entities are not tracked by the current DbContext change tracker.