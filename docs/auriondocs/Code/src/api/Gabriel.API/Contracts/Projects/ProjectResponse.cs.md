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


Represents the request payload to create a new project via the Gabriel API. It groups the required project name with optional metadata such as a description and a system prompt that can configure default assistant behavior for the project. Use this type when constructing a create-project API call instead of passing ad-hoc data; it enforces that a Name is supplied while allowing Description and SystemPrompt to be omitted.

## Remarks
This record acts as a simple Data Transfer Object that defines the contract for a create-project operation. By exposing a stable, typed shape, it decouples API boundaries from domain models and simplifies binding and validation. The Name property is non-nullable, ensuring that every request includes a project name, while Description and SystemPrompt remain optional.

## Notes
- Name is required (non-nullable); supply a non-null string when constructing the instance.
- Description and SystemPrompt are optional and may be null.
- The type is a positional record with constructor parameters ordered as (Name, Description, SystemPrompt), so argument order matters when instantiating it.

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


ProjectFileResponse is an immutable data container (record) used to convey metadata about a single project file in API responses. It exposes the file's Id, Name, SizeBytes, ContentType, and UploadedAt, enabling clients to display and reason about files without transmitting the binary content.

## Remarks
As a record, it provides value-based equality and built-in immutability, which simplifies caching and comparison on the client side and makes it safe to share across API boundaries. The fields chosen focus on metadata that is generally useful for listing and validating files (e.g., display name, size, mime type, and upload time) without coupling to storage specifics. This shape serves as a stable contract between the API and its consumers.

## Notes
- This is metadata-only; the actual file contents are retrieved via a separate download endpoint.
- UploadedAt uses DateTimeOffset to preserve the exact upload moment across time zones; ensure clients display it consistently (e.g., in UTC or a user-specific offset).

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


ProjectResponse is an immutable data transfer object that represents a project's data as returned by the Gabriel API. It bundles identity, descriptive metadata, optional UI overrides, avatar info, lifecycle timestamps, and any associated files, so clients can render project details or drive file/prompt workflows without assembling data from multiple domain models.

## Remarks
This type is a record, ensuring immutability and value-based equality, which makes it safe to share across threads and cache in clients. It provides a stable, API-facing shape that decouples external clients from internal domain models. It contains optional fields (Description, SystemPrompt, PatternOverride, PaletteOverride) to accommodate incomplete data or backward compatibility while still delivering a rich payload when present. Files is an `IReadOnlyList<ProjectFileResponse>` to enforce read-only access and to reflect a one-to-many relationship between a project and its files.

## Notes
- Nullable fields may be null; handle accordingly.
- Files can be null; treat empty as no files.
- AvatarSeed is used to deterministically derive a visual avatar for the project.

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


SetSkinRequest is a tiny data transfer object that carries optional overrides for a skin’s Pattern and Palette. When used in a PUT operation, both fields are sent; a null value clears the corresponding override and reverts to the seed-derived default.

## Remarks
As a small, immutable data carrier, SetSkinRequest cleanly expresses a partial override: you can change Pattern, Palette, both, or clear either independently. The explicit null semantics for each field make it clear whether a caller intends to reset a dimension or keep it as-is, while the controller enforces catalog validity for the override values via SequenceCatalog.

## Notes
- Always include both fields in the payload for PUT semantics; a null value clears the override and falls back to the seed-derived default.
- The controller layer validates catalog identifiers (Pattern and Palette) against SequenceCatalog; ensure those values exist in the catalog before calling this API.

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


UpdateProjectRequest is a lightweight, immutable transport object used to carry optional updates for a project's properties. All fields are nullable, so clients can publish only the fields they want to change (Name, Description, SystemPrompt) without sending a full replacement payload.

## Remarks

Because it is defined as a C# record with positional parameters, UpdateProjectRequest offers concise construction and value-based equality. The nullable fields signal partial updates to the API layer: non-null values indicate new data to apply, while null values preserve existing values. This keeps the API surface small and decoupled from the project domain while still expressing intent clearly.

## Example

```csharp
// Update only the project name
var update = new UpdateProjectRequest(Name: "Nova Project", Description: null, SystemPrompt: null);
```

## Notes

- Nulls typically mean 'do not update this field' in update endpoints; if you need to clear a value, verify how to express that in your API contract (e.g., with a separate flag or by using an explicit empty string).
- Being a record, UpdateProjectRequest is immutable; to represent a new update, construct a new instance rather than mutating an existing one.

---