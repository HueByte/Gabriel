# MemoryRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs`  
> **Kind:** class

```csharp
public class MemoryRepository : IMemoryRepository
```


A concrete EF Core repository implementation of IMemoryRepository that reads and mutates MemoryEntry entities from an AppDbContext. Use this when you want the repository-backed persistence operations (listing, finding, adding, updating, removing) for a specific user's memories; it delegates to the DbContext's MemoryEntries DbSet and applies user/project scoping and ordering logic.

## Remarks
MemoryRepository exists as a thin persistence adapter over AppDbContext: it centralizes common queries that enforce user scoping and the repository's project scoping rules. The important behavioral difference between ListAsync and ListForAgentAsync is intentional: ListAsync returns entries that match the provided projectId exactly (including null), while ListForAgentAsync returns the user's global (projectId == null) entries plus, optionally, entries for a specific project in a single query and orders global entries before project-scoped ones so an agent's UI can display user-scope items first.

## Notes
- ListAsync filters with m.ProjectId == projectId — passing null returns only user-scope (ProjectId == null) entries; it does not return both user-scope and all projects.
- ListForAgentAsync uses a single query with an OR to include user-scope plus (optionally) a specific project's entries, and explicitly orders user-scope entries first (ProjectId == null) before sorting by Type and Name.
- This class manipulates the DbContext's DbSet but does not call SaveChanges/SaveChangesAsync; callers must persist changes on the AppDbContext. Also, AppDbContext/DbContext is not thread-safe — do not share the same context instance concurrently across threads.