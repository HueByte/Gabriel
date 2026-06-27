# ProjectService

> **File:** `src/api/Gabriel.Core/Services/ProjectService.cs`  
> **Kind:** class

A user-scoped service that provides CRUD and utility operations for Project entities. Use this service from application logic or controllers when you need to list, create, update, or delete projects belonging to the currently authenticated user, or when you need the canonical "Default" project to exist for that user.

## Remarks
ProjectService is a thin application/service layer that coordinates between the current user context, the project repository (IProjectRepository) and a unit-of-work (IUnitOfWork). It enforces user scoping on every operation (via a RequireUserId check), ensures a per-user Default project exists (and will backfill legacy conversations to it on first creation), and persists changes by calling the unit-of-work. Methods mutate domain Project instances and delegate persistence to the repository then commit via SaveChangesAsync.

## Example
```csharp
// typical usage in a controller or higher-level service
public class ProjectsController
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    public async Task<IActionResult> CreateAndList(CancellationToken ct)
    {
        // create a new project for the current user
        var created = await _projectService.CreateAsync("My Project", "desc", null, ct);

        // rename it
        await _projectService.RenameAsync(created.Id, "Renamed Project", ct);

        // ensure default exists and list all projects for the user
        var projects = await _projectService.ListAsync(ct);
        return Ok(projects);
    }
}
```

## Notes
- All methods operate in the context of the currently authenticated user; calls will fail if no current user is available (RequireUserId is used internally and will throw/raise an authorization error).
- GetAsync and GetWithFilesAsync throw NotFoundException when the requested project ID does not exist for the current user.
- Mutating operations update the Project entity via the repository and then call IUnitOfWork.SaveChangesAsync to persist changes; callers do not need to call SaveChanges themselves.
- EnsureDefaultProjectIdAsync / EnsureDefaultInternalAsync are idempotent: the first call creates a Default project (and migrates legacy project-less conversations), subsequent calls are cheap and return the existing Default project id.
- All public async methods accept a CancellationToken which is propagated to repository and unit-of-work calls.