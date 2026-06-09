# MemoryRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs`  
> **Kind:** class

Implements IMemoryRepository against an Entity Framework Core AppDbContext to persist and query MemoryEntry entities scoped to a user and an optional project. Use this concrete repository from the infrastructure layer (via DI) when you need to list, find, add, update or remove memory entries tied to a specific user and project; prefer ListForAgentAsync when the caller (an agent UI) needs both user-scoped (projectId == null) and project-scoped entries in a single query.

## Remarks
This repository centralizes the data-access logic for MemoryEntry so callers don't need to construct EF queries themselves. It enforces user scoping on all reads and exposes two list variants: ListAsync (returns entries exactly matching the provided projectId, including null for user-scope) and ListForAgentAsync (returns user-scope plus, optionally, a specific project's entries in one query so the agent UI can fetch both with a single round trip). It does not manage transactions or call SaveChanges; persistence must be finalized by the caller (or an external unit-of-work).

## Example
```csharp
// Typical usage in a service with DI-provided repository and DbContext (or unit-of-work)
public class MemoryService
{
    private readonly IMemoryRepository _repo;
    private readonly AppDbContext _ctx; // or an IUnitOfWork that exposes SaveChangesAsync

    public MemoryService(IMemoryRepository repo, AppDbContext ctx)
    {
        _repo = repo;
        _ctx = ctx;
    }

    public async Task<IReadOnlyList<MemoryEntry>> GetAgentView(Guid userId, Guid? projectId, CancellationToken ct)
    {
        return await _repo.ListForAgentAsync(userId, projectId, ct);
    }

    public async Task AddMemory(MemoryEntry entry, CancellationToken ct)
    {
        await _repo.AddAsync(entry, ct);
        await _ctx.SaveChangesAsync(ct); // repository does not call SaveChanges
    }
}
```

## Notes
- The repository does not call SaveChanges / SaveChangesAsync. Callers are responsible for committing changes to the database.
- AppDbContext (and therefore this repository) is not thread-safe; do not share a single context instance across concurrent threads.
- ListAsync uses exact equality on projectId: passing null returns only user-scoped entries (ProjectId == null). ListForAgentAsync returns user-scope plus the given project when projectId is supplied.
- ListForAgentAsync orders results so user-scoped entries come first (then project-scoped), then by Type and Name; this ordering uses a conditional expression that is translated to SQL by EF Core.