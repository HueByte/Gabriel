# ProjectService

> **File:** `src/api/Gabriel.Core/Services/ProjectService.cs`  
> **Kind:** class

A service that encapsulates project-related use cases for the current user: listing, fetching (with or without files), creating, renaming, updating metadata (description and system prompt), changing appearance (avatar/skin), deleting, and ensuring a per-user "Default" project exists. Use this when you need application-level operations that apply repository access rules, user scoping, and unit-of-work persistence rather than manipulating Project entities or repositories directly.

## Remarks
ProjectService enforces user scoping (each operation requires a current user) and centralizes persistence concerns: it uses an IProjectRepository to query or update Project aggregates and an IUnitOfWork to commit changes. ListAsync proactively ensures a per-user Default project exists (creating and back-filling legacy data on first use) so callers can rely on at least one project being present for a user. Mutating methods load the target Project via the service (which applies the user check and NotFound handling), call domain methods on the Project to perform state changes, update the repository, and then call SaveChangesAsync to persist.

## Example
```csharp
// Typical usage (DI-resolved ProjectService)
var project = await projectService.CreateAsync("My Project", "notes", null, cancellationToken);
project = await projectService.RenameAsync(project.Id, "Renamed", cancellationToken);
var list = await projectService.ListAsync(cancellationToken);
```

## Notes
- All public methods act on the current authenticated user; if no user is present the service will fail (the code requires a user id before proceeding).
- Get/GetWithFiles and other mutating operations will throw NotFoundException when the requested project does not exist or does not belong to the current user.
- ListAsync and EnsureDefaultProjectIdAsync may create a Default project on first call for a user and may perform one-time back-fill for legacy data; subsequent calls are cheap.
- Mutating methods update the repository and rely on IUnitOfWork.SaveChangesAsync to persist changes; callers should observe CancellationToken propagation for long-running operations.
