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


IMemoryService defines a service-layer contract for memory data that derives the current user from ICurrentUser and exposes operations to list, retrieve, upsert, and remove memories within the appropriate scope. It prevents controllers and tools from leaking or manipulating another user's memories by enforcing user-scoped access at the service boundary.

## Remarks
IMemoryService acts as a boundary between controllers/tools and the repository, centralizing memory-access rules and presenting a stable, user-scoped API. It coordinates scope-aware retrieval (including the option to include project-scoped memories for conversations), provides an idempotent SaveAsync upsert, and exposes explicit removal semantics to distinguish between "not found" and "deleted" scenarios.

## Notes
- RemoveAsync and RemoveByNameAsync return a boolean indicating whether a matching entry was removed (false means nothing matched).
- SaveAsync is idempotent: repeated saves with the same spec only update metadata (e.g., UpdatedAt) and do not create duplicate entries.
- ListForConversationAsync returns memories in a deterministic display order (Type, then Name) when presenting data for a given conversation.

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


Represents the specification for a memory entry used by the memory service. MemoryEntrySpec bundles the data required to create or update a memory entry: an optional ProjectId to scope the entry, a Type that classifies the entry (MemoryEntryType), and the entry’s Name, Description, and Body. As a sealed record, it provides value-based equality and immutable semantics, making it a reliable data contract for API boundaries and inter-service communication.

## Remarks
MemoryEntrySpec serves as a transport object between the caller and IMemoryService. It captures the essential metadata and content of a memory entry without embedding persistence concerns. The record nature ensures that two specs with identical properties are treated as equal, which simplifies testing and caching scenarios.

## Example
```csharp
var spec = new MemoryEntrySpec(
    ProjectId: someProjectId,
    Type: MemoryEntryType.Note,
    Name: "Initialization Notes",
    Description: "Prepares the runtime with essential notes",
    Body: "Initialize X, Y, and Z with defaults."
);
```

## Notes
- ProjectId is nullable to permit creating memory entries outside a specific project, or to be assigned later.
- Ensure the Type aligns with the Body semantics (e.g., a CodeSnippet type should have code-like content).
- MemoryEntrySpec is a data contract used by IMemoryService operations; it does not perform persistence itself.

---