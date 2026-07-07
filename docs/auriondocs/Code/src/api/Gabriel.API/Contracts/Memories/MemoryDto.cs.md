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


MemoryDto is the transport-facing representation of a MemoryEntry; this immutable record serves as the data contract for API boundaries and inter-layer transfers, mirroring the MemoryEntry shape. ProjectId null means the memory is user-scoped, while a non-null ProjectId binds it to a specific project and governs visibility inside that project.

## Remarks
This abstraction decouples the external contract from the internal MemoryEntry domain, enabling evolution of the domain model without breaking clients. The optional ProjectId encodes access scope at the boundary, supporting UI or API layers to filter or enforce visibility. CreatedAt and UpdatedAt provide temporal context for synchronization and user interfaces. MemoryDto focuses on serialization-friendly fields suitable for data transfer.

## Example
```csharp
// Example: construct a MemoryDto from a domain entity
MemoryEntry entry = GetMemoryEntryFromRepository(...);
MemoryDto dto = new MemoryDto(
    entry.Id,
    entry.ProjectId,
    entry.Type,
    entry.Name,
    entry.Description,
    entry.Body,
    entry.CreatedAt,
    entry.UpdatedAt);
```

## Notes
- ProjectId may be null; clients should handle this to represent user-scoped memories.
- Body can be large; consider view-level truncation or streaming for list endpoints.
- CreatedAt/UpdatedAt reflect persistence times; ensure correct time zone handling when presenting to users.

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


SaveMemoryRequest is an immutable data transfer object that represents the payload sent to the POST /api/memories endpoint when upserting a memory entry. It carries an optional ProjectId, the memory's Type (restricted to specific categories), a Name, a Description, and the Body content. The endpoint upserts the memory idempotently based on the caller's UserId, the optional ProjectId, and the Name; sending the same request multiple times updates the existing memory rather than creating duplicates.

## Remarks
SaveMemoryRequest exists to validate and transport the essential fields required to create or update a memory record. It cleanly separates transport concerns from the domain model by using a record type; it also makes required fields explicit and supports optional association to a project. The Type field is expected to be one of: "user", "feedback", "project", or "reference".

## Example
```csharp
// Common usage: create a memory associated with a project
var req = new SaveMemoryRequest(
    ProjectId: Guid.Parse("d1a1c6a2-3b2a-4f8b-9a9b-2a8a7e5e7f3b"),
    Type: "reference",
    Name: "Architecture Decision",
    Description: "AD for memory about architecture decision",
    Body: "Details of the architecture decision..."
);
```

```csharp
// Or create a memory without a project association
var req2 = new SaveMemoryRequest(
    ProjectId: null,
    Type: "user",
    Name: "Personal note",
    Description: "Notes about user experience",
    Body: "User preferences and context..."
);
```

## Notes
- UserId is not included in SaveMemoryRequest; the identity used for upsert is derived from the authenticated user context. Ensure your API client supplies the correct authentication context.
- ProjectId is optional; a null value means the memory is not linked to a specific project. Ensure server-side handling aligns with this semantics.
- Type is a free-form string in the payload but is expected to be one of the defined categories; validation should enforce this to avoid invalid memories.


---