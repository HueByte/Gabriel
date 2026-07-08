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


CreateProjectRequest is a value object that represents the payload used to create a new project through the Gabriel API. It exposes a required Name and two optional fields, Description and SystemPrompt, so API clients can supply descriptive metadata and an initial AI system prompt when initializing a project.

## Remarks
By modeling the incoming payload as a record, this symbol provides immutable, value-based equality and a clear contract across boundaries (client to API). The Name field is required by the API, while Description and SystemPrompt are optional to support concise or richer initial context; using this type instead of ad-hoc dictionaries reduces runtime errors and improves IDE support with IntelliSense. It acts as a thin data-transfer boundary that keeps the creation workflow typed and discoverable.

## Notes
- Name is non-nullable in the signature, but the code does not enforce non-empty strings; callers should ensure a meaningful name before sending.
- Description and SystemPrompt are nullable; their presence or absence may affect API behavior—passing null vs omitting the field can be treated differently by the server.

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


Represents metadata for a project file as returned by the API. It carries the file's unique identifier, display name, size in bytes, MIME type, and the timestamp when it was uploaded; use this record when serializing file metadata in project responses.

## Remarks

ProjectFileResponse is an immutable, value-based data carrier (a positional record). This makes it a reliable API contract: equal instances compare by content and instances do not mutate after creation, which simplifies caching and reasoning about data across layers. When evolving this contract, prefer additive changes to preserve compatibility with existing clients.

## Notes

- Id is a Guid and uniquely identifies the file; pass it through API boundaries so clients can reference or fetch the specific file.
- ContentType is a non-nullable string; ensure it is always populated. If the server cannot determine a MIME type, a safe default such as 'application/octet-stream' helps prevent downstream errors during serialization or client rendering.

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


ProjectResponse is an immutable data carrier that represents a snapshot of a project's metadata as returned by the API. It groups identity information (Id, Name), optional descriptive fields (Description, SystemPrompt), visual and presentation settings (AvatarSeed, PatternOverride, PaletteOverride), lifecycle flags (IsDefault), timestamps (CreatedAt, UpdatedAt), and the collection of related files (Files). As a C# record, it favors value-based equality and immutability, making it ideal as a response contract that clients can safely consume without worrying about inadvertent mutations.

## Remarks
This abstraction decouples the API surface from the domain model, providing a stable contract for clients even as internal implementations evolve. Nullable fields reflect optional data that may be omitted by the server (e.g., Description, SystemPrompt, PatternOverride, PaletteOverride, Files). Consumers should handle nulls gracefully or normalise to empty collections where appropriate. The AvatarSeed enables deterministic avatar generation across sessions and clients.

## Example
```csharp
var sample = new ProjectResponse(
    Id: Guid.Empty,
    Name: "Starter Project",
    Description: null,
    SystemPrompt: null,
    AvatarSeed: 123L,
    IsDefault: false,
    PatternOverride: null,
    PaletteOverride: null,
    CreatedAt: DateTimeOffset.UtcNow,
    UpdatedAt: DateTimeOffset.UtcNow,
    Files: Array.Empty<ProjectFileResponse>()
);
```

## Notes
- Nullable fields may be null; check before use. 
- Files may be null or empty; treat null as no files when consuming, or normalise using an empty array/list. 
- Being a record enables with-expressions to create modified copies, preserving immutability while deriving new instances.

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


SetSkinRequest is a small, immutable data carrier used to specify per-dimension skin overrides for a project in the API. It carries two optional fields, Pattern and Palette. Each call to the API includes both fields (PUT semantics). If a field is null, that override is cleared and the system falls back to the seed-derived value for that dimension. When non-null, the provided value overrides the seed behavior for that dimension. The controller layer validates the supplied identifiers against the SequenceCatalog to ensure only known skin components can be applied.

## Remarks
SetSkinRequest acts as a stable contract between clients and the server-side skin logic. By bundling two independent overrides into a single object, it enables clear, idempotent updates where you can enable or disable a specific dimension's override in a single call. It also decouples the API from the underlying storage or domain logic, so changes to how skins are stored won't affect API clients as long as the contract remains the same.

## Example
```csharp
// Set both overrides
var request = new SetSkinRequest("FloralPattern", "VividPalette");

// Clear only the Pattern override (Palette remains as provided)
var clearPattern = new SetSkinRequest(null, "VividPalette");

// Clear both overrides (revert to seeds for both)
var reset = new SetSkinRequest(null, null);
```

## Notes
- Null values clear the override for that dimension.
- Both fields are sent on every PUT request; you cannot omit one field to indicate no change.
- The API validates the provided identifiers against SequenceCatalog at the controller boundary to ensure only known skin components are applied.

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


UpdateProjectRequest is a data transfer object used to convey changes to a project's metadata. It exposes three nullable properties—Name, Description, and SystemPrompt—allowing callers to specify only the fields they want to update while leaving others untouched.

## Remarks
This record represents a partial-update contract for a project. By modeling all fields as nullable, it enables patch-style updates against a Projects API without the need for multiple specialized payload types. It serves as a focused abstraction that decouples update semantics from the full project representation, making intent explicit when performing update operations.

## Example
```csharp
// Update only the Name
var req1 = new UpdateProjectRequest("NewName", null, null);

// Update Name and SystemPrompt
var req2 = new UpdateProjectRequest("NewName", null, "System prompt text");

// Update Description only
var req3 = new UpdateProjectRequest(null, "New description", null);
```

## Notes
- The constructor is positional; to omit a field, pass null explicitly for that parameter.
- Whether a null value means "no change" or triggers a specific API behavior is determined by the consuming API contract; the record itself does not encode this semantics.


---