# IMemoryService.cs

> **Source:** `src/api/Gabriel.Core/Services/IMemoryService.cs`

## Contents

- [IMemoryService](#imemoryservice)
- [MemoryEntrySpec](#memoryentryspec)

---

## IMemoryService
> **File:** `src/api/Gabriel.Core/Services/IMemoryService.cs`  
> **Kind:** interface

```csharp
public interface IMemoryService
```


IMemoryService is a service-layer contract that enforces user-scoped access to memories by pulling the caller's UserId from the current user context and delegating to the repository. It exposes operations to list, retrieve, save (upsert), and remove MemoryEntry items within an optional project scope, so controllers and tools don’t have to thread user identity through every call.

## Remarks
Architecturally, it acts as a boundary between controllers/tools and IMemoryRepository, centralizing scope and authorization concerns. ListAsync returns all memories the current user has within the given project scope (null means user-only). ListForConversationAsync combines the user-scoped memories with the conversation's project-scoped memories, when applicable, and presents them in a stable display order by Type and then by Name. SaveAsync implements an idempotent upsert based on UserId, ProjectId, and Name: repeated saves with the same spec do not create duplicates and only update the UpdatedAt timestamp. RemoveAsync returns a boolean indicating whether a target was actually deleted, while RemoveByNameAsync deletes by name within an optional project scope.

## Notes
- This interface assumes an authenticated user context; without a resolvable UserId, scope enforcement cannot be performed.
- Null projectId means user-scoped operations; providing a projectId scopes the operation to that project.
- SaveAsync is idempotent: a second SaveAsync call with the same spec is a no-op aside from bumping UpdatedAt.

---

## MemoryEntrySpec
> **File:** `src/api/Gabriel.Core/Services/IMemoryService.cs`  
> **Kind:** record

```csharp
public sealed record MemoryEntrySpec(
    Guid? ProjectId,
    MemoryEntryType Type,
    string Name,
    string Description,
    string Body)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `ProjectId` | `Guid?` | — |
| `Type` | [`MemoryEntryType`](../Entities/MemoryEntryType.cs.md) | — |
| [`Name`](../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `Description` | `string` | — |
| `Body` | `string` | — |


MemoryEntrySpec is an immutable data contract that describes a memory entry to be persisted by the memory service. It aggregates an optional ProjectId, the entry Type, a human-friendly Name, a Description, and the content Body. Use this record when creating or updating memory entries to ensure all required metadata and content are provided in a single, transportable payload; its value-based semantics help keep comparisons reliable across layers.

## Remarks
MemoryEntrySpec centralizes the shape of a memory entry into a single, reusable payload. It couples the project scope, the kind of memory (MemoryEntryType), and the descriptive fields with the actual content, enabling consistent handling by the IMemoryService. As a sealed record, it provides value equality and deconstruction-friendly access, making tests and data transfer straightforward while preventing unintended inheritance.

## Notes
- ProjectId is nullable; when null, the entry has no explicit project scope and may be treated as global or unscoped depending on the service policy.
- Name, Description, and Body should be provided with meaningful content; the memory service will typically validate required fields and may reject entries that are empty or null.

---