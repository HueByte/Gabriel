# IProjectRepository

> **File:** `src/api/Gabriel.Core/Repositories/IProjectRepository.cs`  
> **Kind:** interface

Represents a persistence boundary for Project aggregates and related queries/commands scoped to a specific owning user. Use this interface from application services when you need to load, list, create, update, delete, or perform a bulk assignment of conversations to a Project without depending on a concrete ORM or database API.

## Remarks
This is a repository abstraction that centralizes Project-related data access and enforces owner scoping via the ownerUserId parameter on read operations. It also exposes a targeted bulk operation (AssignOrphanConversationsAsync) referenced in the source as part of the "Default-project" lazy backfill flow — implementations are expected to efficiently assign any conversations that currently lack a project to the specified project.

## Example
```csharp
// Example usage inside an application service
public class ProjectService
{
    private readonly IProjectRepository _projects;

    public ProjectService(IProjectRepository projects)
    {
        _projects = projects;
    }

    public async Task<Project?> GetProjectAsync(Guid ownerUserId, Guid projectId, CancellationToken ct)
    {
        return await _projects.GetByIdWithFilesAsync(projectId, ownerUserId, ct);
    }

    public async Task CreateAndBackfillAsync(Guid ownerUserId, Project project, CancellationToken ct)
    {
        await _projects.AddAsync(project, ct);
        // After creating a default project, assign orphan conversations to it
        await _projects.AssignOrphanConversationsAsync(ownerUserId, project.Id, ct);
    }
}
```

## Notes
- Methods that return Project? can return null if no matching entity is found — callers must handle a null result.
- ownerUserId is required on all read queries and is used to scope results to a specific user's data (multi-tenant/ownership guard).
- Update and Remove are synchronous on the interface; exact persistence semantics (immediate save vs. unit-of-work) depend on the concrete implementation and should be understood before relying on side effects.