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


MemoryDto is the wire representation of a single MemoryEntry used for transferring memory data across API boundaries. It mirrors the MemoryEntry properties that are relevant to clients but remains a plain data container with no domain behavior. As a sealed record, it is immutable and supports value-based equality, which makes it convenient to compare and propagate memory items across layers. The Id identifies the memory; ProjectId, if set, ties the memory to a particular project (and constrains visibility to that project), while a null ProjectId indicates a user-scoped memory. The remaining fields (Type, Name, Description, Body) describe the memory, and CreatedAt / UpdatedAt provide lightweight auditing information.

## Remarks
MemoryDto serves as the API contract for Memory data, decoupled from the domain entity MemoryEntry. This separation enables the API surface to evolve independently from domain rules while still carrying the same essential data. Because MemoryDto is immutable, you create new instances to reflect changes rather than mutating existing ones; use mapping from MemoryEntry when persisting or presenting data.

## Notes
- Null ProjectId indicates user-scoped memory; clients should handle absence gracefully.
- MemoryDto is a data-transfer object with no behavior; avoid relying on mutations to trigger logic; for updates, construct a new DTO or map to domain and back.

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


SaveMemoryRequest is the API payload used to persist (or upsert) a memory entry via POST /api/memories. It carries an optional ProjectId to scope the memory, a Type that categorizes the memory, and the required Name, Description, and Body. The operation is idempotent and keyed on the composite (UserId, ProjectId, Name); sending the same values again will upsert the existing memory rather than creating a duplicate.

## Remarks
This symbol acts as a plain data carrier at the API boundary, isolating transport concerns from domain logic. The UserId is implied from the authenticated context, while ProjectId scopes the memory when provided. Modeling as a record emphasizes its role as data used to perform an idempotent upsert, enabling straightforward value-based comparisons in tests.

## Example
```csharp
var request = new SaveMemoryRequest(
    ProjectId: Guid.Empty,
    Type: "reference",
    Name: "LaunchChecklist",
    Description: "Checklist used during the launch phase",
    Body: "1) Confirm readiness; 2) Verify systems; 3) Approve to proceed..."
);
```

## Notes
- Validate that Type is one of the allowed values (user, feedback, project, reference) before sending; the server should enforce this, per the contract.
- ProjectId is nullable; omit it to store a user-scoped memory or provide a project GUID to scope to a project. The effective identity for the upsert also depends on the authenticated UserId.
- SaveMemoryRequest is a sealed record, so instances are immutable. To adjust any field, construct a new instance rather than mutating an existing one.

---