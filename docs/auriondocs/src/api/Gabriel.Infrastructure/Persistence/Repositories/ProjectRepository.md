# ProjectRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/ProjectRepository.cs`  
> **Kind:** class

Repository that manages Project entities using an AppDbContext and Entity Framework Core. Use this when you need data access operations for Project records scoped to a specific owner (multi-tenant / per-user), including fetching projects with their files, adding/updating/removing projects, listing projects for an owner, and bulk-assigning orphaned conversations to a project. This class delegates persistence to the provided AppDbContext and does not call SaveChanges itself.

## Remarks
ProjectRepository is a thin EF Core-backed repository that centralizes common queries and mutations for the Project aggregate. It consistently filters by ownerUserId to enforce per-user scoping, exposes async query methods that translate to database operations, and uses EF Core's bulk update API (ExecuteUpdateAsync) to reassign conversations without loading them into the change tracker. Callers are expected to manage transaction boundaries and call SaveChangesAsync (or use a unit-of-work) when they need to persist Add/Update/Remove operations.

## Example
```csharp
// Resolve via DI or construct with an AppDbContext
var repo = new ProjectRepository(dbContext);

// List projects for a user
var projects = await repo.ListAsync(ownerUserId, cancellationToken);

// Get a project and its files (files included and ordered by UploadedAt desc)
var projectWithFiles = await repo.GetByIdWithFilesAsync(projectId, ownerUserId, cancellationToken);

// Add a new project (remember to save changes on the context)
await repo.AddAsync(newProject, cancellationToken);
await dbContext.SaveChangesAsync(cancellationToken);

// Bulk assign conversations that currently have no ProjectId to this project
int affected = await repo.AssignOrphanConversationsAsync(ownerUserId, projectId, cancellationToken);
```

## Notes
- None of the mutation methods (AddAsync, Update, Remove) call SaveChanges; the caller must persist changes on the AppDbContext.
- GetByIdWithFilesAsync uses Include(p => p.Files.OrderByDescending(...)) to request files ordered by UploadedAt descending; whether the related collection is materialized in that exact order can depend on EF Core behavior and projection; verify ordering if your code depends on it.
- AssignOrphanConversationsAsync performs a server-side bulk update with ExecuteUpdateAsync and returns the number of rows affected. It does not load or update tracked Conversation entities in the current DbContext instance.
- All public query methods filter by ownerUserId — this enforces per-user scoping but callers must pass the correct owner id to avoid unexpected empty results.
