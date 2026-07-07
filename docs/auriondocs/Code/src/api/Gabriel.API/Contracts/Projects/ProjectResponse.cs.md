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

```csharp
public record CreateProjectRequest(string Name, string? Description, string? SystemPrompt)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| [`Name`](../../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `Description` | `string?` | — |
| `SystemPrompt` | `string?` | — |


CreateProjectRequest is a data contract used to request the creation of a new project through the Gabriel API. It encapsulates the required project name and two optional pieces of metadata: Description and SystemPrompt. Use this record whenever you need to send a strongly-typed payload to the CreateProject endpoint, instead of assembling a raw JSON object by hand. The immutability of records helps ensure the payload remains consistent across layers.

## Remarks
Because this is a C# record, instances are immutable value objects with structural equality, making them safe to pass across layers and use in caches or comparisons. The SystemPrompt field seeds the initial behavior for AI workflows associated with the project, while Description provides human-friendly context. If you need a modified payload, use the with-expression to produce a new CreateProjectRequest rather than mutating an existing instance.

## Notes
- Description and SystemPrompt are optional and may be null; server-side code should handle nulls gracefully.
- Name is non-nullable and should be provided with a meaningful value; empty strings may be rejected by the API.
- Be mindful of SystemPrompt length and sensitive content; avoid logging the prompt in logs or telemetry.

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


Represents the information returned for a single project file in API responses. This record provides a compact, serializable payload that conveys the file's identity, name, size, MIME type, and upload timestamp so clients can display or validate project files after operations like upload or listing.

## Remarks
This symbol acts as a stable, serializable data contract that decouples API responses from internal storage models. By using a record, it benefits from value-based equality and immutability, making it safe to pass around as a simple data carrier across endpoints. The chosen fields (Id, Name, SizeBytes, ContentType, UploadedAt) cover identification, presentation, and temporal context needed by clients.

## Notes
- SizeBytes is the file size in bytes; use this value to format human-friendly sizes on the client.
- UploadedAt uses DateTimeOffset to preserve the exact point in time in a consistent offset; consumers should consider time zone handling when displaying.
- ContentType should reflect the MIME type of the file; if unknown, default to "application/octet-stream" and validate before use.

---

## ProjectResponse
> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

```csharp
public record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    string? SystemPrompt,
    long AvatarSeed,
    bool IsDefault,
    string? PatternOverride,
    string? PaletteOverride,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ProjectFileResponse>? Files)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Id` | `Guid` | — |
| [`Name`](../../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `Description` | `string?` | — |
| `SystemPrompt` | `string?` | — |
| `AvatarSeed` | `long` | — |
| `IsDefault` | `bool` | — |
| `PatternOverride` | `string?` | — |
| `PaletteOverride` | `string?` | — |
| `CreatedAt` | `DateTimeOffset` | — |
| `UpdatedAt` | `DateTimeOffset` | — |
| `Files` | `IReadOnlyList<ProjectFileResponse>?` | — |


ProjectResponse is the API-facing data contract that bundles a project's core metadata and its related files into a single, serializable object. It serves as a stable, value-based transfer object used when returning project data from the Gabriel API, including identity (Id), display data (Name, Description), optional AI system prompt (SystemPrompt), UI customization hooks (AvatarSeed, PatternOverride, PaletteOverride), lifecycle timestamps (CreatedAt, UpdatedAt), and the collection of file details (Files).

## Remarks
ProjectResponse exists to provide a stable API-facing envelope around a project's metadata and its files, decoupling the contract from domain entities so it can evolve without leaking internal details. The Files property leverages ProjectFileResponse to surface file-level information while keeping project metadata and file data as distinct concerns.

## Example
```csharp
var sample = new ProjectResponse(
    Id: Guid.NewGuid(),
    Name: "Core Platform",
    Description: "Main platform project",
    SystemPrompt: "Be concise and helpful.",
    AvatarSeed: 42,
    IsDefault: true,
    PatternOverride: null,
    PaletteOverride: null,
    CreatedAt: DateTimeOffset.UtcNow.AddDays(-7),
    UpdatedAt: DateTimeOffset.UtcNow,
    Files: new List<ProjectFileResponse>()
);
```

## Notes
- Description and SystemPrompt may be null; handle accordingly.
- Files may be null; if so, treat as an empty list to simplify consumption.

---

## SetSkinRequest
> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

```csharp
public record SetSkinRequest(string? Pattern, string? Palette)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| [`Pattern`](../../../../webapp/src/pulse/patterns.ts.md) | `string?` | — |
| `Palette` | `string?` | — |


SetSkinRequest is a lightweight payload used to apply per-dimension overrides to a project's skin configuration. Both Pattern and Palette are optional; each call includes these fields. If a field is null, that dimension's override is cleared and the system falls back to the seed-derived value for that aspect; the controller validates the provided identifiers against the SequenceCatalog.

## Remarks
SetSkinRequest isolates client intent from the domain logic that builds a skin from seeds and catalog-provided options. It enables partial updates and a clear contract for how overrides are applied on each request. By using null to clear, it ensures callers can revert to the default behavior without resending full configuration.

## Example
```csharp
// Clear the Pattern override, keep using seed-derived Palette
var req = new SetSkinRequest(null, "Sunset");

// Apply both overrides
var req2 = new SetSkinRequest("Stripes", "Aurora");
```

## Notes
- Ensure the JSON serializer emits null values so that clearing an override is conveyed. 
- When binding, both fields must be provided in the payload; omitting a field may be treated as a no-op depending on serializer configuration.


---

## UpdateProjectRequest
> **File:** `src/api/Gabriel.API/Contracts/Projects/ProjectResponse.cs`  
> **Kind:** record

```csharp
public record UpdateProjectRequest(string? Name, string? Description, string? SystemPrompt)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| [`Name`](../../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string?` | — |
| `Description` | `string?` | — |
| `SystemPrompt` | `string?` | — |


Represents the payload used to update a project's properties via the update endpoint. This record carries optional values for Name, Description, and SystemPrompt, allowing callers to modify only the fields they want to change. Because the properties are nullable, a null value signals that the corresponding field should be left unchanged during the update. The record is immutable, so a new instance must be created with the desired values rather than mutating an existing one.

## Remarks
This symbol serves as a concise contract between client and server for partial updates. By accepting nullable fields, it enables partial updates without requiring the full project object to be resent. It sits alongside other API contracts to express update semantics without embedding domain logic; the server interprets the non-null values as replacements for existing fields while preserving others.

## Notes
- The record is immutable; to change any field, construct a new UpdateProjectRequest instance.
- The constructor requires three arguments; to indicate no change for a field, pass null for that parameter.

---