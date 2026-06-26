# ProjectRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/ProjectRepository.cs`  
> **Kind:** class

A thin EF Core repository that encapsulates common persistence operations for Project aggregates and project-scoped queries. Use this when you want a small, testable abstraction over AppDbContext for reading and modifying projects and for performing a bulk reassign of orphaned conversations owned by a specific user.

## Remarks
This repository centralizes project-related queries and simple mutations so higher-level services do not directly depend on AppDbContext or repeat common filtering (notably scoping by ownerUserId). It intentionally leaves transaction and save semantics to the caller: AddAsync, Update and Remove only manipulate the EF change tracker (and AddAsync only enqueues the entity), while AssignOrphanConversationsAsync uses a server-side bulk update to avoid loading rows into memory.

## Example
```csharp
// resolve repository (e.g. via DI) and a DbContext or unit-of-work that exposes SaveChangesAsync
var projects = await projectRepository.ListAsync(currentUserId, cancellationToken);

// create and persist a new project
var project = new Project { Name = "New", OwnerUserId = currentUserId };
await projectRepository.AddAsync(project, cancellationToken);
await dbContext.SaveChangesAsync(cancellationToken);

// reassign orphan conversations in bulk (returns number of rows updated)
int updated = await projectRepository.AssignOrphanConversationsAsync(currentUserId, project.Id, cancellationToken);
await dbContext.SaveChangesAsync(cancellationToken);

// load project with files
var projectWithFiles = await projectRepository.GetByIdWithFilesAsync(project.Id, currentUserId, cancellationToken);
```

## Notes
- Repository methods do not call SaveChanges/SaveChangesAsync; callers must persist changes on the DbContext/UnitOfWork.
- Most query methods filter by ownerUserId — this enforces per-user scoping and should be supplied correctly to avoid leaking or missing data.
- AssignOrphanConversationsAsync uses ExecuteUpdateAsync to perform a server-side bulk update without tracking; this returns the number of rows affected and requires EF Core support for ExecuteUpdateAsync (EF Core 7+).
- The Include with OrderByDescending (GetByIdWithFilesAsync) relies on EF Core behavior for ordered collection materialization; ordering guarantees can vary by provider/version, so do not rely on it for ordering-sensitive business logic unless verified for your EF Core version.
