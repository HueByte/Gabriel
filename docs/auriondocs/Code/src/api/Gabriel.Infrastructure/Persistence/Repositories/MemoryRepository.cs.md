# MemoryRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs`  
> **Kind:** class

Provides an Entity Framework Core–backed repository for persisting and querying MemoryEntry records scoped to a user (and optionally a project). Use this class when you want a thin data-access abstraction over AppDbContext for listing, finding, adding, updating, and removing memory entries; higher-level services typically consume this repository and are responsible for committing changes.

## Remarks
This is a straightforward EF Core repository implementing IMemoryRepository. ListAsync returns entries matching an explicit (possibly null) project scope; ListForAgentAsync is written to return both user-scoped entries (ProjectId == null) and, when a project is provided, that project’s entries in a single query so the caller only pays one database round-trip. The repository performs ordering so user-scoped entries appear first, then by Type and Name.

## Example
```csharp
// typical usage inside an application service
var repo = new MemoryRepository(dbContext);
var entries = await repo.ListForAgentAsync(userId, projectId, cancellationToken);

var newEntry = new MemoryEntry { Id = Guid.NewGuid(), UserId = userId, ProjectId = projectId, Name = "note", Type = "text" };
await repo.AddAsync(newEntry, cancellationToken);
await dbContext.SaveChangesAsync(cancellationToken); // commit is required
```

## Notes
- AddAsync/Update/Remove do not call SaveChanges/SaveChangesAsync; the caller must commit the DbContext to persist changes.
- AppDbContext (DbContext) is not thread-safe. Do not use the same repository instance concurrently from multiple threads.
- Equality checks against ProjectId use SQL equality semantics; null projectId matches only null ProjectId.
- String comparisons (e.g., FindByNameAsync) follow the database collation, so case-sensitivity depends on DB configuration.