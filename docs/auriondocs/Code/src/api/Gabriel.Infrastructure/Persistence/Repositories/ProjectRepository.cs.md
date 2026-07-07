# ProjectRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/ProjectRepository.cs`  
> **Kind:** class

```csharp
public class ProjectRepository : IProjectRepository
```


A concrete EF Core repository that provides CRUD and query operations for Project entities scoped to a specific owner user. Use this when you want a repository abstraction over AppDbContext for reading, adding, updating, removing projects and for bulk-assigning orphan conversations to a project.

## Remarks
ProjectRepository wraps an AppDbContext to implement IProjectRepository by scoping queries to an ownerUserId (multi-tenant / per-user isolation) and exposing common patterns: single-entity fetches, a fetch that includes related Files (ordered by Upload time), listing ordered by UpdatedAt, and simple add/update/remove operations. The AssignOrphanConversationsAsync method uses a set-based database update (ExecuteUpdateAsync) to reassign conversations whose ProjectId is null without loading them into the EF change tracker, which improves performance for bulk updates.

## Example
```csharp
// Typical usage within an application service
var repo = new ProjectRepository(appDbContext);
await repo.AddAsync(new Project { Id = Guid.NewGuid(), OwnerUserId = userId, Name = "Demo" }, ct);
await appDbContext.SaveChangesAsync(ct); // repository methods do not call SaveChanges

var project = await repo.GetByIdWithFilesAsync(projectId, userId, ct);
if (project != null)
{
    project.Name = "Updated";
    repo.Update(project);
    await appDbContext.SaveChangesAsync(ct);
}

// Bulk assign orphan conversations to a project
await repo.AssignOrphanConversationsAsync(userId, projectId, ct);
```

## Notes
- Repository methods (AddAsync, Update, Remove) do not call SaveChanges; the caller is responsible for persisting changes.
- GetByIdWithFiles includes the Files collection ordered by UploadedAt; depending on EF Core version, Include with ordering requires EF Core support for filtered/ordered includes.
- AssignOrphanConversationsAsync performs a server-side bulk update and does not populate the change tracker for affected Conversation entities; any tracked Conversation instances will not reflect the update until the context is refreshed or detached and reloaded.