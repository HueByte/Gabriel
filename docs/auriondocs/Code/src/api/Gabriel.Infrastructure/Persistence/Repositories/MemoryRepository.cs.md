# MemoryRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs`  
> **Kind:** class

```csharp
public class MemoryRepository : IMemoryRepository
```


A lightweight EF Core-backed repository that implements IMemoryRepository for querying and mutating MemoryEntry entities scoped to a user (and optionally to a project). Use this when you want database-backed access to memories with the repository handling common query shapes (list by project, agent view that mixes user/global and project-scoped entries) instead of writing LINQ against AppDbContext directly.

## Remarks
MemoryRepository is a thin persistence adapter over AppDbContext.MemoryEntries. It intentionally performs queries as IQueryable -> ToListAsync so callers get materialized, ordered collections and the class does not call SaveChanges/SaveChangesAsync; callers are responsible for committing changes after AddAsync/Update/Remove. ListForAgentAsync encodes the agent-facing view: it returns the user's global (project-null) entries plus, when a project id is supplied, that project's entries in a single SQL query and orders results so user-scope (global) entries appear first, then project-scoped entries, with secondary ordering by Type then Name.

## Notes
- AddAsync/Update/Remove only change the DbContext state; persistence requires an explicit save on the DbContext (e.g. SaveChangesAsync) outside this repository.
- ListAsync uses equality on ProjectId (m.ProjectId == projectId). To retrieve only global (user-scoped) entries call with projectId == null; to retrieve only one project's entries pass that project's id.
- FindByNameAsync matches Name and ProjectId exactly within the given UserId — it will not search across both global and project scopes. ListForAgentAsync is the method to use when you need the combined agent view (global + optional project) in one round-trip.