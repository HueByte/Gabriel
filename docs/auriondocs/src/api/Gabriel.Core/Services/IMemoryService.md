# IMemoryService.cs

> **Source:** `src/api/Gabriel.Core/Services/IMemoryService.cs`

## Contents

- [IMemoryService](#imemoryservice)
- [MemoryEntrySpec](#memoryentryspec)

---

## IMemoryService

> **File:** `src/api/Gabriel.Core/Services/IMemoryService.cs`  
> **Kind:** interface

A service-level abstraction that exposes user-scoped memory operations (list, read, upsert, delete) while pulling the current caller's UserId from ICurrentUser and enforcing user/project scoping. Use IMemoryService from controllers, tools, or agents when you want safe, caller-scoped access to memories without passing or trusting external user identifiers.

## Remarks
This interface is the security and convenience boundary over IMemoryRepository: it centralizes scoping rules (current user and optional project), composes user- and project-scoped results for conversation views, and provides an idempotent upsert contract for saves. It also returns explicit boolean results for deletes so callers (including tools and agents) can distinguish between "not found" and "deleted" outcomes.

## Example
```csharp
// Typical usage inside a controller or tool handler
public async Task<IActionResult> SaveMemory(MemoryEntrySpec spec, CancellationToken ct)
{
    // IMemoryService infers the current user; callers don't supply a user id.
    var saved = await _memoryService.SaveAsync(spec, ct);
    return Ok(saved);
}

public async Task<IActionResult> ShowConversationMemories(Guid? projectId, CancellationToken ct)
{
    var entries = await _memoryService.ListForConversationAsync(projectId, ct);
    // entries are already combined and sorted for display (Type, then Name)
    return Ok(entries);
}

public async Task<IActionResult> RemoveMemoryByName(Guid? projectId, string name, CancellationToken ct)
{
    var removed = await _memoryService.RemoveByNameAsync(projectId, name, ct);
    if (!removed) return NotFound();
    return NoContent();
}
```

## Notes
- Pass projectId = null to operate in user-scope only for ListAsync and other operations.
- SaveAsync performs an upsert keyed by (UserId, ProjectId, Name) and is idempotent; repeating the same spec will only update timestamps such as UpdatedAt.
- RemoveAsync and RemoveByNameAsync return false when no matching entry exists — callers should use this to provide a clear "wasn't there" response.
- All methods accept a CancellationToken; forward one from callers where appropriate.

---

## MemoryEntrySpec

> **File:** `src/api/Gabriel.Core/Services/IMemoryService.cs`  
> **Kind:** record

An immutable data transfer object that describes a memory entry (its optional project scope, type, name, description and body). Use this record when creating or updating memory entries via the memory service API to provide all required fields in a single value object.

## Remarks
This sealed record centralizes the fields required to represent a memory entry payload passed to memory-related operations. Being a positional record it provides value-based equality, deconstruction, and concise construction syntax; use the `with` expression to produce modified copies rather than mutating an instance.

## Example
```csharp
// Project-scoped memory entry
var spec = new MemoryEntrySpec(
    ProjectId: Guid.Parse("f2b5d6e1-3a4b-4c2d-9f7a-1234567890ab"),
    Type: MemoryEntryType.Note,
    Name: "Design decision: caching",
    Description: "Rationale for choosing in-memory caching",
    Body: "We chose a local in-memory cache because ..."
);

// Global (no project) memory entry
var globalSpec = spec with { ProjectId = null, Name = "Global: FAQ" };
```

## Notes
- ProjectId is nullable: pass null for a global (non-project) memory entry.
- The record is sealed and immutable; use `with` to create a modified copy.
- The string properties are non-nullable in the signature — callers should provide non-null values for Name, Description, and Body.


---