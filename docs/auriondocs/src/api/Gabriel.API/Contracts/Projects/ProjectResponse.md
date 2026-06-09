# ProjectResponse.cs

> **Source:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`

## Contents

- [CreateProjectRequest](#createprojectrequest)
- [ProjectFileResponse](#projectfileresponse)
- [ProjectResponse](#projectresponse)
- [SetSkinRequest](#setskinrequest)
- [UpdateProjectRequest](#updateprojectrequest)

---

## CreateProjectRequest

> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

A minimal API contract that represents the data required to create a new project: a required project Name and two optional text fields (Description and SystemPrompt). Use this record when accepting or forwarding create-project payloads in the API layer or service boundaries where an immutable, value-based DTO is preferred.

## Remarks
This is a compact positional record that provides immutable properties, value-based equality, deconstruction, and built-in constructor-based initialization. It is intended as a transport/model type for create operations — it carries only the initial metadata needed to create a project and does not include persistence or identity fields (those are provided by the created resource returned from the server).

## Example
```csharp
// Constructing a request to create a new project
var req = new CreateProjectRequest(
    Name: "Research Assistant",
    Description: "Project for assisting research tasks",
    SystemPrompt: "You are a helpful research assistant."
);

// Example passing to a service/controller method
await projectService.CreateAsync(req);
```

## Notes
- Name is declared non-nullable; callers and model binders must supply a value. Omitting it during binding will typically result in a model validation error.
- Description and SystemPrompt are nullable; consumers should handle potential null values.
- This positional record relies on constructor-based initialization. JSON deserialization will bind by constructor parameter names (most serializers used in ASP.NET Core perform case-insensitive matching by default).

---

## ProjectFileResponse

> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

```csharp
public record ProjectFileResponse(
    Guid Id,
    string Name,
    long SizeBytes,
    string ContentType,
    DateTimeOffset UploadedAt)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Id` | `Guid` | — |
| [`Name`](../../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `SizeBytes` | `long` | — |
| `ContentType` | `string` | — |
| `UploadedAt` | `DateTimeOffset` | — |


Represents metadata for a file attached to a project and is used as a response DTO in the API. It carries the file's unique identifier, display name, size in bytes, MIME content type, and the timestamp when the file was uploaded. Use this contract when returning file listings or metadata; the actual file contents are not included.

## Remarks
This is a compact, positional record intended for transport over API boundaries. As a record it provides value-based equality and convenient deconstruction, making it suitable for comparisons in tests and for simple mapping scenarios. It exists to separate file metadata from file payload delivery (downloads/streams), keeping responses lightweight.

## Notes
- SizeBytes is the file size in bytes and may be large; use a 64-bit integer to avoid overflow.
- ContentType is a MIME type (for example, "image/png" or "application/pdf"); treat it as authoritative for client-side handling but do not rely on it for security checks.
- UploadedAt is a DateTimeOffset, preserving the original offset; normalize to UTC if you need consistent cross-timezone comparisons.

---

## ProjectResponse

> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

Represents the data returned by the API for a project: its identity, display metadata, optional UI overrides, timestamps, and an optional list of associated files. Use this record as the canonical read-only shape when returning project details from endpoints or when transporting project data between layers.

## Remarks
This is a positional C# record designed as an immutable data-transfer object (DTO) for API responses. It groups core metadata (Id, Name, Description), presentation overrides (PatternOverride, PaletteOverride, AvatarSeed), timestamps (CreatedAt, UpdatedAt), and an optional collection of ProjectFileResponse items. The record-based shape provides value equality and concise construction for consumers and servers exchanging project information.

## Example
```csharp
var response = new ProjectResponse(
    Id: Guid.NewGuid(),
    Name: "Website Revamp",
    Description: "Project for redesigning the public website",
    SystemPrompt: null,
    AvatarSeed: 42L,
    IsDefault: false,
    PatternOverride: "grid",
    PaletteOverride: "ocean",
    CreatedAt: DateTimeOffset.UtcNow,
    UpdatedAt: DateTimeOffset.UtcNow,
    Files: new List<ProjectFileResponse>() // or null
);
```

## Notes
- Several properties are nullable: Description, SystemPrompt, PatternOverride, PaletteOverride, and Files may be null when that information is not present.
- Files is typed as `IReadOnlyList<ProjectFileResponse>`?: the interface prevents modification through the property but does not guarantee the underlying collection is immutable.
- Being a record, instances use value-based equality (by-property) and are intended as immutable snapshots of project state.

---

## SetSkinRequest

> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

Represents a request to set or clear a project's skin overrides for two dimensions: Pattern and Palette. This DTO is used when replacing the skin configuration (PUT semantics) — both properties are expected to be present in the request; set a property to null to clear that override and fall back to the seed-derived behavior for that dimension.

## Remarks
Validation of the provided identifiers is performed at the controller layer against the SequenceCatalog; this record does not perform any validation itself. Because the API uses PUT semantics here, the intent is to replace the existing skin overrides in full rather than apply a partial/merge update.

## Example
```csharp
// Set a specific pattern and clear any palette override (fallback to seed-derived palette)
var req = new SetSkinRequest("pattern-catalog-id", null);

// Clear both overrides (use seed-derived pattern and palette)
var clearReq = new SetSkinRequest(null, null);
```

## Notes
- Null has semantic meaning: it clears the override for that dimension. Omitting a property is not supported for partial updates here — both fields should be included in the request body.
- Unknown or invalid catalog identifiers will be rejected by the controller (validation against SequenceCatalog).
- This is an immutable record used as a simple transport DTO; it does not perform normalization or validation itself.

---

## UpdateProjectRequest

> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

Represents a lightweight, immutable request DTO for updating a project's mutable fields. Each property is nullable so callers can supply only the values they intend to change; use this when sending partial update payloads to an API endpoint rather than passing a full project model.

## Remarks
This positional record is a simple carrier of optional values (Name, Description, SystemPrompt) with no validation or update logic. It is intended for use as a contract between clients and server endpoints that accept partial updates; the handling of null (whether it means "leave unchanged" or "clear the value") is decided by the update handler, not by this type.

## Example
```csharp
// Update only the project name, leaving other fields unchanged (handler decides semantics for nulls)
var req = new UpdateProjectRequest(Name: "New Project Name", Description: null, SystemPrompt: null);

// Serialize to JSON for an HTTP PATCH/PUT body
var json = System.Text.Json.JsonSerializer.Serialize(req);
// json might look like: {"Name":"New Project Name","Description":null,"SystemPrompt":null}

// Handler signature example (ASP.NET Core controller)
// public IActionResult UpdateProject(Guid id, UpdateProjectRequest request) { ... }
```

## Notes
- Confirm the intended null semantics with the API: some handlers treat null as "no change", others treat it as "clear the field".
- This record carries no validation; callers or the receiving endpoint should validate lengths, required-when-present rules, and other business constraints.
- As a positional record, its properties are immutable (init-only) after construction.

---