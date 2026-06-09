# MemoryDto.cs

> **Source:** `src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs`

## Contents

- [MemoryDto](#memorydto)
- [SaveMemoryRequest](#savememoryrequest)

---

## MemoryDto

> **File:** `src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs`  
> **Kind:** record

Represents the wire shape of a memory exposed by the API. This DTO is used when serializing or deserializing Memory objects across the API boundary (for requests and responses) and mirrors the domain MemoryEntry for transport scenarios.

## Remarks
This sealed record decouples the API contract from the internal MemoryEntry domain model so the public surface remains stable even if the domain object changes. Being a record provides value-based equality and convenient immutability; use mapping code to translate between the domain model and this transport shape.

## Example
```csharp
// Create a user-scoped memory DTO (ProjectId is null for user-scope)
var dto = new MemoryDto(
    Id: Guid.NewGuid(),
    ProjectId: null,
    Type: "note",
    Name: "Grocery list",
    Description: "Items to buy",
    Body: "Milk\nEggs\nBread",
    CreatedAt: DateTimeOffset.UtcNow,
    UpdatedAt: DateTimeOffset.UtcNow
);
```

## Notes
- ProjectId == null indicates a user-scoped memory; a non-null ProjectId ties the memory to that project and makes it visible only inside that project.
- The record is immutable; use a with-expression or create a new instance to represent updates.
- CreatedAt and UpdatedAt are DateTimeOffset values and retain timezone/offset information — normalize on the consumer side if needed.
- Body can contain free-form text and may be large; consider payload size limits when sending/receiving this DTO.


---

## SaveMemoryRequest

> **File:** `src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs`  
> **Kind:** record

A DTO used to create or update a "memory" via the POST /api/memories endpoint. Reach for this record when sending a client's request body to save a memory; the server treats the operation as an idempotent upsert (so repeat submissions with the same identity will update rather than duplicate).

## Remarks
The API performs an idempotent upsert keyed by (UserId, ProjectId, Name). ProjectId is nullable — provide null when the memory is not scoped to a specific project. The Type field is a string expected to be one of: "user", "feedback", "project", or "reference". The authenticated user is inferred by the API and is not part of this DTO.

## Example
```csharp
// Save a project-scoped feedback memory
var req = new SaveMemoryRequest(
    ProjectId: projectId,               // Guid? — null if not project-scoped
    Type: "feedback",                  // one of: "user","feedback","project","reference"
    Name: "Homepage usabilty note",
    Description: "Observed during testing session #12",
    Body: "Users struggled with the signup flow when X happened..."
);
// POST req as JSON to /api/memories
```

## Notes
- Name, Description and Body are required by the API contract but not enforced by the C# type system; validate before sending.
- Type is a free-form string in this DTO; use one of the documented values ("user","feedback","project","reference").
- ProjectId == null indicates the memory is not tied to a project.
- Idempotency is based on the combination of authenticated UserId (provided by server), ProjectId, and Name — repeated requests with the same key update the existing memory rather than creating duplicates.


---