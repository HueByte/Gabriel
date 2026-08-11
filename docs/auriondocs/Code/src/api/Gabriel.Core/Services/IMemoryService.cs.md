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


IMemoryService provides a user-scoped, domain-level API for managing MemoryEntry records. It serves as a boundary between controllers/tools and the persistence layer, pulling the current UserId from the context and enforcing user-scoped access so tools cannot inadvertently read or modify memories outside the caller's scope.

## Remarks
IMemoryService centralizes memory operations behind a user-aware facade, isolating authentication/authorization concerns from higher layers. It coordinates with the repository and MemoryEntry entities to offer upsert, retrieval, and removal semantics that respect both user and project scopes, while exposing convenient methods tailored to common use cases (e.g., displaying memories for a conversation with both user- and project-scoped memories).

## Notes
- Be mindful of cancellation via the provided CancellationToken; propagate it through to repository calls to avoid unresponsive operations.
- RemoveAsync returns false when no entry matched, so callers can distinguish between a no-op and an actual deletion. RemoveByNameAsync similarly indicates absence of a match; handle these booleans gracefully in UI/flows.

## Dependencies
- MemoryEntry
- IMemoryRepository
- UpdatedAt

## Dependency APIs (verified signatures)
- class [`MemoryEntry`](../Entities/MemoryEntry.cs.md) (`src/api/Gabriel.Core/Entities/MemoryEntry.cs`)
  - property `Guid Id`
  - property `Guid UserId`
  - property `Guid? ProjectId`
  - property `MemoryEntryType Type`
  - property `string Name`
  - property `string Description`
  - property `string Body`
  - property `DateTimeOffset CreatedAt`
  - property `DateTimeOffset UpdatedAt`
  - `MemoryEntry()`
  - `MemoryEntry Create(Guid userId, Guid? projectId, MemoryEntryType type, string name, string description, string body)`
  - `void Update(MemoryEntryType type, string description, string body)`
  - …and 1 more member(s) not shown
- interface [`IMemoryRepository`](../Repositories/IMemoryRepository.cs.md) (`src/api/Gabriel.Core/Repositories/IMemoryRepository.cs`)
  - `Task<IReadOnlyList<MemoryEntry>> ListAsync(Guid userId, Guid? projectId, CancellationToken ct)`
  - `Task<IReadOnlyList<MemoryEntry>> ListForAgentAsync(Guid userId, Guid? projectId, CancellationToken ct)`
  - `Task<MemoryEntry?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct)`
  - `Task<MemoryEntry?> FindByNameAsync(Guid userId, Guid? projectId, string name, CancellationToken ct)`
  - `Task AddAsync(MemoryEntry entry, CancellationToken ct)`
  - `void Update(MemoryEntry entry)`
  - `void Remove(MemoryEntry entry)`
- property `UpdatedAt` (`src/api/Gabriel.Core/Entities/Conversation.cs`)

## Symbol To Document
- Name: `IMemoryService`
- Kind: interface
- File: `src/api/Gabriel.Core/Services/IMemoryService.cs`
- Language: csharp
- ID: fe3d8046-665f-4fb4-9056-a057b2b417d2

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


MemoryEntrySpec is an immutable data contract used to describe a single memory entry within the memory service. It carries an optional project association through ProjectId, the entry type through Type, and the essential metadata required for display or storage: Name, Description, and Body. As a record, it provides value-based equality and convenient copying semantics, making it ideal for transferring memory-entry data between layers without introducing behavior.

## Remarks
MemoryEntrySpec serves as a boundary between API/domain concerns and persistence. It encapsulates the minimal data needed to create or update an entry and can be used to validate input before mapping to domain entities. The nullable ProjectId allows entries to be associated with a project when present, while Type distinguishes the kind of memory entry without requiring call sites to interpret the raw fields. Since it's a C# record, it benefits from value-based equality and supports non-destructive modification via with-expressions to create altered copies while preserving the original instance. The Body content represents the actual textual payload of the memory entry.

---