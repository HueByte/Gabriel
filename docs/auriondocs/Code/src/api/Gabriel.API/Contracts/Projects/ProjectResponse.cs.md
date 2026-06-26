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

Represents the data sent to the API to create a new project. Use this DTO when calling the create-project endpoint or when binding incoming request bodies; Name is required (non-null), while Description and SystemPrompt are optional and may be null.

## Remarks
This is a simple, immutable C# record used as an API contract/DTO. It carries only the minimal fields needed to create a project and relies on the receiving endpoint or service to perform validation (for example, to ensure Name is not empty) and to map the values into the domain model or persistence layer. Because it is a positional record, equality is value-based and properties are init-only.

## Example
```csharp
// Constructing the request directly
var req = new CreateProjectRequest(
    Name: "Research Assistant",
    Description: "Project for academic literature summarization",
    SystemPrompt: "You are a helpful research assistant."
);

// Typical use in an ASP.NET controller action (model binding)
[HttpPost]
public IActionResult CreateProject([FromBody] CreateProjectRequest request)
{
    // validate request.Name, create domain entity, persist, return response
}
```

## Notes
- The record does not perform validation: callers should ensure Name is provided and meets any length/format rules expected by the API.
- Description and SystemPrompt are nullable; their absence should be handled by the server (e.g., use defaults or leave fields empty).
- As a record, instances are immutable after creation and use value-based equality, which is convenient for testing and DTO comparisons.

---

## ProjectFileResponse

> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

Represents metadata for a file attached to a project. Use this record when returning file details from API endpoints or serializing project file lists—it's a lightweight, value-based DTO that conveys identity, name, size, content type, and upload timestamp.

## Remarks
This record is part of the API contract surface for project-related endpoints and is intended as a read-only data transfer shape. It emphasizes value semantics (records compare by value) and immutability, making it convenient for returning file metadata from services without exposing internal storage details.

## Example
```csharp
var file = new ProjectFileResponse(
    Id: Guid.NewGuid(),
    Name: "design-specs.pdf",
    SizeBytes: 184_320,
    ContentType: "application/pdf",
    UploadedAt: DateTimeOffset.UtcNow);

// returning as part of a project response
var files = new[] { file };
return Ok(new { ProjectId = projectId, Files = files });
```

## Notes
- SizeBytes is measured in bytes (useful for display or quota calculations).
- ContentType is expected to be a MIME type (e.g., "image/png", "application/pdf").
- UploadedAt is a DateTimeOffset so callers receive the timestamp together with its offset; the record is immutable and uses value-based equality (use with-expressions to produce modified copies).

---

## ProjectResponse

> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

Represents a project returned by the API — a read-only data-transfer object carrying project identity, metadata, optional presentation overrides, timestamps, and an optional list of associated files. Reach for this type when consuming project information from project-related endpoints (it is the shape returned by the API), rather than using domain entities directly.

## Remarks
This is a positional C# record intended as an API contract: it exposes immutable, init-only properties and implements value-based equality so two instances with the same data compare equal. Files is typed as an `IReadOnlyList<ProjectFileResponse>`? to signal that the file collection is provided for read-only consumption and may be absent (null) when no files are included.

## Example
```csharp
var project = new ProjectResponse(
    Id: Guid.NewGuid(),
    Name: "Demo Project",
    Description: "A short description",
    SystemPrompt: null,
    AvatarSeed: 42L,
    IsDefault: false,
    PatternOverride: null,
    PaletteOverride: null,
    CreatedAt: DateTimeOffset.UtcNow,
    UpdatedAt: DateTimeOffset.UtcNow,
    Files: Array.Empty<ProjectFileResponse>()
);
```

## Notes
- Files may be null; check for null before iterating.
- As a positional record, properties are init-only and instances are effectively immutable after construction.
- Equality is value-based: two records with identical property values are considered equal.

---

## SetSkinRequest

> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

A compact DTO used to set or clear a project's visual skin by specifying catalog identifiers for a pattern and a palette. Use this record when sending a skin update request to the API; providing null for a field removes any override and returns that dimension to seed-derived behavior.

## Remarks
This record follows PUT-style semantics: both Pattern and Palette are expected to be supplied on each call. Passing null for either property clears any existing override for that dimension so the project falls back to the seed-derived value. The string values are catalog identifiers; actual validation of those identifiers is performed at the controller layer against the SequenceCatalog.

## Example
```csharp
// Set both pattern and palette by catalog id
var req = new SetSkinRequest("pattern-123", "palette-azure");

// Clear only the palette override (pattern remains set)
var clearPalette = new SetSkinRequest("pattern-123", null);

// Clear both overrides (revert both to seed-derived behavior)
var clearBoth = new SetSkinRequest(null, null);
```

## Notes
- The record does not validate catalog ids itself; the API controller validates against SequenceCatalog and will reject unknown identifiers.
- Clients should send both fields on each update (even if unchanged) because omission is not treated differently from sending null.
- Null has semantic meaning (clear override) — an empty string is not the same and will be treated as a literal identifier.

---

## UpdateProjectRequest

> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

A simple data-transfer object used to carry updated project metadata from a client to the server. Each property is nullable so callers can supply only the fields they intend to change (commonly used with a PATCH-like update endpoint).

## Remarks
This positional record serves as the API contract for updating a project's mutable fields (name, description and the system prompt). Making the properties nullable allows the API to distinguish between "no change" (null) and an explicit update (including an empty string). The record is intentionally minimal — validation and merge semantics are handled by the update endpoint or service layer.

## Example
```csharp
// Update only the project's name
var request = new UpdateProjectRequest(Name: "New Project Name", Description: null, SystemPrompt: null);

// Using with-expression to create a modified copy of an existing request
var modified = request with { Description = "Updated description" };
```

## Notes
- Null vs empty string: null typically means "leave the existing value unchanged"; an empty string may be interpreted as clearing the value. Confirm the API's specific merge semantics before sending.
- Immutability: this is a positional record — instances are immutable; use a with-expression to create modified copies.
- Serialization: some JSON serializers may omit null properties by default, which affects payload size and how the server interprets absent vs null fields. Ensure client and server agree on serializer settings.

---