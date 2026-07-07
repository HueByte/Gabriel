# MemoryDto.cs

> **Source:** `src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs`

## Contents

- [MemoryDto](#memorydto)
- [SaveMemoryRequest](#savememoryrequest)

---

## MemoryDto
> **File:** `src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs`  
> **Kind:** record

```csharp
public sealed record MemoryDto(
    Guid Id,
    Guid? ProjectId,
    string Type,
    string Name,
    string Description,
    string Body,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Id` | `Guid` | — |
| `ProjectId` | `Guid?` | — |
| `Type` | `string` | — |
| [`Name`](../../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `Description` | `string` | — |
| `Body` | `string` | — |
| `CreatedAt` | `DateTimeOffset` | — |
| `UpdatedAt` | `DateTimeOffset` | — |


MemoryDto is the API-facing data contract for a single memory entry; it mirrors the MemoryEntry entity's visible fields so clients can read or write memory data, while omitting internal user ownership. The nullable ProjectId signals scope: null means user-scoped, non-null ties the memory to that project and restricts visibility accordingly.

## Remarks
MemoryDto serves as a boundary object between the domain model and API clients. By including CreatedAt and UpdatedAt, it provides timing metadata without exposing internal identifiers like UserId, and the optional ProjectId communicates visibility scope without leaking ownership across boundaries. When mapping MemoryEntry instances for API responses or requests, map Id, Type, Name, Description, Body, CreatedAt, UpdatedAt, and ProjectId into MemoryDto.

## Notes
- Be aware that ProjectId == null marks user-scoped memories; clients should treat null as private to the user and apply project-based filters accordingly.

---

## SaveMemoryRequest
> **File:** `src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs`  
> **Kind:** record

```csharp
public sealed record SaveMemoryRequest(
    Guid? ProjectId,
    string Type,
    string Name,
    string Description,
    string Body)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `ProjectId` | `Guid?` | — |
| `Type` | `string` | — |
| [`Name`](../../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `Description` | `string` | — |
| `Body` | `string` | — |


SaveMemoryRequest is a data transfer object used as the body payload for POST /api/memories. It carries the memory metadata and content that will be upserted; the server uses an idempotent upsert keyed on (UserId, ProjectId, Name). The UserId is derived from the caller's authentication context, and is not part of this payload. The object requires Name, Description, and Body, while Type discriminates the memory category and must be one of: user, feedback, project, or reference. The optional ProjectId allows scoping the memory to a particular project; when omitted, the memory can be considered not bound to a project. This is a sealed, immutable record, ensuring value-based equality and predictable data transfer across API boundaries.

## Remarks
This DTO exists to keep the HTTP boundary clean: a simple, language-native representation of the memory to be stored. By using a record, the payload remains immutable and easy to compare across layers, which helps with change detection and testing. The nullable ProjectId provides flexibility for project-scoped or global/test memories, while Name acts as a stable identifier within the given scope. The Type field being a string (rather than an enum) preserves forward-compatibility with potential new categories without requiring API surface changes.

## Example
```csharp
var request = new SaveMemoryRequest(
    ProjectId: Guid.NewGuid(),
    Type: "project",
    Name: "IntroNotes",
    Description: "High-level notes for the project kickoff",
    Body: "{ \"summary\": \"Initial summary of project goals and milestones\" }"
);
```

## Notes
- Type must be one of the allowed values; validation should occur in the API layer since the field is a plain string here.  
- ProjectId is nullable; when null, the memory may be treated as not tied to a specific project.  
- Ensure Description and Body contain content that is safe for storage and display, considering length limits and sanitization as required by the API.

---