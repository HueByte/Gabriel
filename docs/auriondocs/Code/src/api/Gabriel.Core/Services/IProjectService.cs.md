# IProjectService

> **File:** `src/api/Gabriel.Core/Services/IProjectService.cs`  
> **Kind:** interface

Provides asynchronous operations for managing a user's projects: listing, retrieving (with or without files), creating, renaming/updating metadata, setting avatar/skin, deleting, and ensuring a per-user "Default" project exists. Reach for this interface when implementing or calling the domain/service layer responsible for project lifecycle and project-scoped metadata; it centralizes behaviors such as lazy creation/migration of a Default project and avatar/skin handling.

## Remarks
This interface is the application-facing abstraction for project management and is intended to be implemented by the service layer that coordinates persistence, business rules, and any migration of legacy data. ListAsync and EnsureDefaultProjectIdAsync have intentional side effects: they lazily ensure a user-scoped Default project exists and will absorb legacy project-less conversations on first invocation. Avatar/skin behavior is split between RerollAvatarAsync (changes the seed so seed-derived pattern/palette change) and SetSkinAsync (pins or clears explicit pattern/palette overrides); catalog identifier validation is expected to occur at the API layer before calling SetSkinAsync. All operations are asynchronous and accept CancellationToken to allow cooperative cancellation.

## Example
```csharp
// Typical usage inside an API controller or higher-level service
var projects = await projectService.ListAsync(ct);
var defaultId = await projectService.EnsureDefaultProjectIdAsync(ct);
var newProject = await projectService.CreateAsync("My Project", "notes", null, ct);
await projectService.SetSkinAsync(newProject.Id, pattern: "patternId", palette: "paletteId", ct);
var fullProject = await projectService.GetWithFilesAsync(newProject.Id, ct); // files are newest-first
```

## Notes
- ListAsync and EnsureDefaultProjectIdAsync may mutate data (create the Default project and reassign legacy conversations); callers should expect and tolerate that side effect.
- GetWithFilesAsync returns files sorted newest-first; consumers do not need to re-sort for that ordering.
- SetSkinAsync accepts null or empty to clear an override; callers outside the API should validate catalog identifiers because the implementation assumes API-layer validation.
- All methods are asynchronous; pass a CancellationToken where appropriate to avoid blocking on long-running I/O operations.
