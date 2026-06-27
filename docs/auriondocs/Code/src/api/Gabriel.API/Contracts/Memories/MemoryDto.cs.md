# MemoryDto.cs

> **Source:** `src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs`

## Contents

- [MemoryDto](#memorydto)
- [SaveMemoryRequest](#savememoryrequest)

---

## MemoryDto

> **File:** `src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs`  
> **Kind:** record

A simple, immutable data-transfer object that describes the wire shape of a memory as exposed by the API. Use this record when sending or receiving memory representations over HTTP (for example in controller responses or request/response payloads) rather than exposing internal domain types directly.

## Remarks
This DTO intentionally mirrors the API contract for a memory: identity, optional project scoping, metadata, content, and timestamps. It exists to separate the external contract from internal/domain models (e.g., MemoryEntry) so the API can evolve or validate independently. The record is immutable which makes it a good fit for serialization and thread-safe publishing.

## Example
```csharp
// Constructing a DTO (e.g., in a controller or mapper)
var dto = new MemoryDto(
    Id: Guid.NewGuid(),
    ProjectId: null, // null => user-scoped memory
    Type: "note",
    Name: "Grocery list",
    Description: "Items to buy",
    Body: "Milk, Eggs, Bread",
    CreatedAt: DateTimeOffset.UtcNow,
    UpdatedAt: DateTimeOffset.UtcNow);

// Returning from an ASP.NET Core controller
// return Ok(dto);
```

## Notes
- ProjectId == null denotes a user-scoped memory; a non-null ProjectId confines visibility to that project.
- Type is a plain string (not an enum) — do not assume a fixed set of values unless validated elsewhere.
- The record is immutable; to produce one from a domain MemoryEntry, map fields explicitly (or use a mapping helper/constructor).

---

## SaveMemoryRequest

> **File:** `src/api/Gabriel.API/Contracts/Memories/MemoryDto.cs`  
> **Kind:** record

Represents the request payload for creating or updating a "memory" via POST /api/memories. Use this record when sending the API a memory to save; the endpoint performs an idempotent upsert (the server determines uniqueness by the tuple (UserId, ProjectId, Name)). Body, Description and Name are required by the API and Type must be one of: "user", "feedback", "project", or "reference".

## Remarks
This is a thin DTO intended for transport only; it does not include authentication information. The comment above the type indicates the upsert key includes UserId, but UserId is expected to be derived from the authenticated context on the server (not supplied in this record). ProjectId is nullable to allow memories that are scoped to the user (null) or to a specific project (non-null). The server is responsible for validating required fields and enforcing allowed Type values.

## Example
```csharp
// C# construction of the DTO for a project-scoped memory
var req = new SaveMemoryRequest(
    ProjectId: Guid.Parse("d2b9f4a1-3c4f-4aad-9d2a-1a2b3c4d5e6f"),
    Type: "project",
    Name: "Design Decisions - Auth",
    Description: "Summary of the auth design choices.",
    Body: "Full text describing why JWT was chosen over sessions..."
);

// Equivalent JSON payload sent to POST /api/memories
// (shown here as JSON but use the same fields when calling the API)
/*
{
  "projectId": "d2b9f4a1-3c4f-4aad-9d2a-1a2b3c4d5e6f",
  "type": "project",
  "name": "Design Decisions - Auth",
  "description": "Summary of the auth design choices.",
  "body": "Full text describing why JWT was chosen over sessions..."
}
*/
```

## Notes
- The DTO itself does not enforce "required" semantics; the server will reject requests missing Name, Description or Body.  
- Type is a string and must match one of the allowed values (user, feedback, project, reference); expect server-side validation and rejection for unknown values.  
- Because UserId is not part of this payload, calls must be authenticated and the server should use the authenticated user as the identity when performing the idempotent upsert keyed by (UserId, ProjectId, Name).

---