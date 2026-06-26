# IMemoryService.cs

> **Source:** `src/api/Gabriel.Core/Services/IMemoryService.cs`

## Contents

- [IMemoryService](#imemoryservice)
- [MemoryEntrySpec](#memoryentryspec)

---

## IMemoryService

> **File:** `src/api/Gabriel.Core/Services/IMemoryService.cs`  
> **Kind:** interface

A service-layer abstraction over the memory repository that centralizes user-scoping and common memory operations for controllers and tools. Use this interface when you need to list, retrieve, upsert, or remove memory entries on behalf of the current user without plumbing the caller's UserId through every call; the implementation reads the current user (ICurrentUser) and enforces per-user isolation at this boundary.

## Remarks
This interface intentionally sits above IMemoryRepository to enforce security and scoping rules: callers provide only the optional project context (projectId) and the service attaches the calling user's identity. It also provides higher-level behaviors useful to callers: merging user-scope and project-scope memories for conversations, returning results in display order, and performing idempotent upserts (create-or-update) so tools and controllers can call SaveAsync without first checking existence.

## Example
```csharp
// List user-only memories
var userMemories = await memoryService.ListAsync(projectId: null, ct);

// List what an agent should see for a conversation (includes both user and project scope)
var visible = await memoryService.ListForConversationAsync(projectId: conversationProjectId, ct);

// Upsert a memory entry (create or update in-place)
var saved = await memoryService.SaveAsync(new MemoryEntrySpec {
    ProjectId = projectId,
    Name = "favorite_color",
    Type = "preference",
    Value = "blue"
}, ct);

// Remove by name within a project (or user-scope when projectId is null)
var removed = await memoryService.RemoveByNameAsync(projectId, "favorite_color", ct);
```

## Notes
- Passing projectId = null targets the calling user's personal (user-scope) memories only; non-null projectId includes project-scoped entries. 
- ListForConversationAsync returns the union of user-scope memories and (when applicable) the conversation's project-scope memories, sorted for display by Type then Name.
- SaveAsync performs an upsert keyed by (UserId, ProjectId, Name); repeated calls with the same spec are idempotent aside from updating the entry's UpdatedAt timestamp.
- RemoveAsync returns false when no entry matched (true only on an actual delete)—useful for clear tooling responses.
- All operations accept a CancellationToken; callers should forward cancellation where appropriate.

---

## MemoryEntrySpec

> **File:** `src/api/Gabriel.Core/Services/IMemoryService.cs`  
> **Kind:** record

Represents the data required to create or describe a memory entry handled by the memory service. Use this immutable, positional record when you need to pass the project scope, entry type, human-friendly name, description, and the entry body to IMemoryService methods (for example when adding or updating memory entries).

## Remarks
This sealed positional record is a compact Data Transfer Object that groups the common fields the memory subsystem needs. Its value-based equality and deconstruct support make it convenient for passing around, comparing, and pattern-matching entries. ProjectId is nullable to allow entries that are not scoped to a specific project.

## Example
```csharp
// create a new memory entry specification (using named arguments for clarity)
var spec = new MemoryEntrySpec(
    ProjectId: projectId,                      // Guid? — null for global/unscoped
    Type: MemoryEntryType.Note,               // MemoryEntryType enum value
    Name: "Meeting notes",
    Description: "Notes from the 2026-06-01 planning meeting",
    Body: "Decisions: ...\nAction items: ..."
);

// deconstructing
var (projId, type, name, description, body) = spec;

// pass to a memory service
await memoryService.AddEntryAsync(spec);
```

## Notes
- The record is sealed and positional: it provides immutable properties, value equality, and deconstruction.
- ProjectId is nullable; a null value typically indicates an unscoped or global entry.
- The type and string properties are not validated by this type — callers must enforce constraints (length, allowed characters, business rules) before sending to the service.
- Strings are non-nullable by the signature; do not pass null for Name, Description, or Body.

---