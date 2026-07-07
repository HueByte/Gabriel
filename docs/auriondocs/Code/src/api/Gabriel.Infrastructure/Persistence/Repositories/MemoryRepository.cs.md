# MemoryRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs`  
> **Kind:** class

```csharp
public class MemoryRepository : IMemoryRepository
```


A repository implementation that provides query and basic mutation operations for MemoryEntry entities scoped to a user and (optionally) a project. Use this when you need to list, find, add, update or remove memory entries from the application's AppDbContext while enforcing user and project scoping rules; it does not itself persist changes to the database (SaveChanges must be called on the DbContext).

## Remarks
MemoryRepository encapsulates common persistence patterns for MemoryEntry: user-scoped access, optional project scoping, and an "agent"-focused listing that returns both global (user-level) and project-level memories in one efficient query. The specialized ListForAgentAsync method builds a single IQueryable that conditionally includes project-scoped entries alongside user-scoped (null project) entries and orders them so user-scope entries appear first, which keeps round-trips to the database predictable and minimal.

## Example
```csharp
// Typical read use from a service with the repository injected
var memories = await memoryRepository.ListForAgentAsync(userId, projectId, cancellationToken);

// Adding a new entry and persisting it via the DbContext
await memoryRepository.AddAsync(new MemoryEntry { /* ... */ }, cancellationToken);
await appDbContext.SaveChangesAsync(cancellationToken);

// Updating or removing
memoryRepository.Update(existingEntry);
memoryRepository.Remove(existingEntry);
await appDbContext.SaveChangesAsync(cancellationToken);
```

## Notes
- The repository methods (AddAsync, Update, Remove) only affect the DbContext state; callers must call SaveChanges/SaveChangesAsync on the AppDbContext to persist changes.
- AppDbContext (and therefore this repository) is not thread-safe. Do not use the same MemoryRepository instance concurrently from multiple threads.
- Equality and ordering behavior (e.g. FindByNameAsync exact string match, projectId null handling, and the conditional OrderBy in ListForAgentAsync) are delegated to the underlying database provider and its collation/translation rules; results may vary with provider semantics.