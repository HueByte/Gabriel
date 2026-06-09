# MemoryService

> **File:** `src/api/Gabriel.Core/Services/MemoryService.cs`  
> **Kind:** class

A service that provides CRUD operations for per-user memory entries and enforces that every operation is performed on behalf of an authenticated user. Use this service from application/business layers when you need to list, retrieve, create/update (upsert by name), or remove memory entries without dealing directly with repository or unit-of-work wiring.

## Remarks
MemoryService centralizes user-scoped memory management by delegating storage work to an IMemoryRepository and committing changes through an IUnitOfWork. It enforces the current user requirement via ICurrentUser and throws an UnauthorizedAccessException when no user is present. SaveAsync implements an "upsert by name" flow: it finds an existing entry (by user, project, and name) and updates it if present, otherwise creates a new entry and persists changes through the unit of work.

## Example
```csharp
// Typical usage in an application service or controller (injected via DI)
public async Task UseMemory(IMemoryService memoryService)
{
    var spec = new MemoryEntrySpec
    {
        ProjectId = Guid.NewGuid(),
        Type = MemoryType.Custom,
        Name = "user-preferences",
        Description = "Preferences for the user",
        Body = "{...}"
    };

    // SaveAsync will create or update the entry for the current user
    var entry = await memoryService.SaveAsync(spec);

    // Remove by name (returns false if not found)
    var removed = await memoryService.RemoveByNameAsync(spec.ProjectId, spec.Name);
}
```

## Notes
- All operations require an authenticated user; if ICurrentUser.UserId is null, RequireUserId() throws UnauthorizedAccessException.
- SaveAsync uses name + project (scoped to the current user) to decide whether to create or update — callers should ensure names are used consistently if uniqueness matters.
- Methods return boolean for removals to indicate whether an entry was found and deleted; repositories perform the actual ownership checks and persistence is completed only after IUnitOfWork.SaveChangesAsync is called.