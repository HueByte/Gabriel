# IProjectService

> **File:** `src/api/Gabriel.Core/Services/IProjectService.cs`  
> **Kind:** interface

Provides high-level operations for managing a user's projects (scoped to the repository / multi-tenant context). Use this interface from application or API layers to list, retrieve, create, update, delete and perform avatar/skin operations on projects. Prefer this abstraction when you need repository-scoped project semantics (including lazy creation of a per-user "Default" project and migration of legacy project-less conversations) rather than manipulating project entities directly.

## Remarks
This service centralizes user-scoped project lifecycle and related behaviors so callers do not need to know migration or seeding details. It lazily ensures a "Default" project exists and, on first use, will absorb legacy (pre-Phase-8) conversations into that default project. Avatar operations are provided at the project level: RerollAvatarAsync changes the seed used to generate derived avatar dimensions, while SetSkinAsync pins or clears explicit pattern/palette overrides (validation of catalog identifiers is done before calling this service at the API layer).

## Example
```csharp
// Typical usage inside an async handler or controller
public async Task UseProjectService(IProjectService projects, CancellationToken ct)
{
    // Ensure the user has a default project and get its id
    Guid defaultId = await projects.EnsureDefaultProjectIdAsync(ct);

    // Create a new project
    var newProject = await projects.CreateAsync("Notes", "quick captures", null, ct);

    // Rename and update system prompt
    await projects.RenameAsync(newProject.Id, "Work Notes", ct);
    await projects.UpdateSystemPromptAsync(newProject.Id, "You are an assistant for note-taking.", ct);

    // Pin a skin (pass null or empty to clear an override)
    await projects.SetSkinAsync(newProject.Id, pattern: "stripes", palette: "warm", ct);

    // List projects for the current user (this will lazily ensure Default exists)
    var all = await projects.ListAsync(ct);
}
```

## Notes
- Methods are asynchronous and support CancellationToken; callers should propagate cancellation to avoid long-running work.
- ListAsync and EnsureDefaultProjectIdAsync have side effects: they lazily create the per-user "Default" project and will migrate legacy project-less conversations on first invocation.
- SetSkinAsync treats null or empty strings as "clear the override" for that dimension; catalog identifier validation is expected to occur before calling into this service.
- GetWithFilesAsync returns the project with its files loaded and sorted newest-first (caller should not rely on an additional ordering step).