# ModelDto.cs

> **Source:** `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`

## Contents

- [ModelDto](#modeldto)
- [ModelsResponse](#modelsresponse)
- [SelectedModelDto](#selectedmodeldto)
- [SetActiveModelRequest](#setactivemodelrequest)

---

## ModelDto
> **File:** `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`  
> **Kind:** record

```csharp
public sealed record ModelDto(
    string Provider,
    string Name,
    int ContextWindowTokens,
    
    
    
    double? CompactThreshold,
    decimal InputPricePerMTokens,
    decimal OutputPricePerMTokens,
    decimal CacheReadPricePerMTokens,
    decimal CacheWritePricePerMTokens,
    bool IsDefault,
    bool IsSelected)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Provider` | `string` | — |
| [`Name`](../../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `ContextWindowTokens` | `int` | — |
| `CompactThreshold` | `double?` | — |
| `InputPricePerMTokens` | `decimal` | — |
| `OutputPricePerMTokens` | `decimal` | — |
| `CacheReadPricePerMTokens` | `decimal` | — |
| `CacheWritePricePerMTokens` | `decimal` | — |
| `IsDefault` | `bool` | — |
| `IsSelected` | `bool` | — |


ModelDto is a data carrier that represents one model entry from the /api/models response, including per‑million-token pricing and an optional per‑model CompactThreshold. Use it when listing or pricing models returned by the API and when a per-model override should govern behavior instead of the global AgentOptions, while also tracking the provider, identity, context window, and selection state.

## Remarks
ModelDto decouples API shape from internal pricing and UI logic by encapsulating all relevant metadata for a model in a single, immutable record. It enables consumers to surface model options, compare costs, and respect per-model overrides without scattering token pricing logic across callers. The nullable CompactThreshold allows providers with tiered pricing to express a model-specific trigger while falling back to a global threshold when not set.

## Example
```csharp
var dto = new ModelDto(
    Provider: "OpenAI",
    Name: "gpt-4",
    ContextWindowTokens: 8192,
    CompactThreshold: 0.18,
    InputPricePerMTokens: 0.03m,
    OutputPricePerMTokens: 0.02m,
    CacheReadPricePerMTokens: 0.0m,
    CacheWritePricePerMTokens: 0.0m,
    IsDefault: true,
    IsSelected: false);
```

## Notes
- CompactThreshold being null means the global AgentOptions.CompactThreshold is used; callers should resolve this default before computing triggers.
- Cache pricing fields are zero when the provider does not expose caching pricing; rely on API documentation for availability.
- Prices are per-million tokens (MTokens); multiply accordingly for your usage.

---

## ModelsResponse
> **File:** `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`  
> **Kind:** record

```csharp
public sealed record ModelsResponse(
    IReadOnlyList<ModelDto> AvailableModels,
    SelectedModelDto Selected)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `AvailableModels` | `IReadOnlyList<ModelDto>` | — |
| `Selected` | `SelectedModelDto` | — |


ModelsResponse defines the API payload returned by GET /api/models. It carries two essential pieces of information: AvailableModels, the complete list of model options exposed by the catalog; and Selected, the model the agent will currently use for the user (defaulting to the configured value if the user hasn’t explicitly chosen one yet). As an immutable sealed record, it provides a stable, single-communication contract that couples the catalog with the user’s active choice.

## Remarks

This abstraction cleanly separates the catalog of available models from the per-user selection, enabling independent evolution of the model catalog and the selection logic. It provides a stable contract for clients to render both the full option set and the current selection in a single response, reducing round-trips and keeping UI concerns aligned with the server state.

## Example

```csharp
// Typical usage in an API controller
var allModels = _catalogService.GetAllModels();       // IReadOnlyList<ModelDto>
var selection = _userContext.GetSelectedModel(userId); // SelectedModelDto
var response = new ModelsResponse(allModels, selection);
return Ok(response);
```

## Notes

- Ensure AvailableModels is a non-null read-only collection to preserve immutability from the API surface.
- Selected should always reflect the user’s current choice, or the configured default when none is set.
- The shape of this payload is part of the public API contract; avoid changing property names or types in a backward-incompatible way.

---

## SelectedModelDto
> **File:** `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`  
> **Kind:** record

```csharp
public sealed record SelectedModelDto(string Provider, string Name)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Provider` | `string` | — |
| [`Name`](../../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string` | — |


SelectedModelDto is a compact, immutable data transfer object that carries the essential details of a model selection: the provider that supplies the model and the model's name. Implemented as a sealed positional record, it provides value-based equality and concise construction, making it ideal for API contracts and boundaries where a consumer selects a specific model.

## Remarks
As a record, it gains built-in value equality, deconstruction support, and non-destructive copy via with-expressions. Being sealed communicates that this type is a simple data container not intended for inheritance. Its two properties, Provider and Name, form the complete identity of the selection and are typically serialized in API payloads.

## Example
```csharp
// Common usage: create and inspect a selection
var selection = new SelectedModelDto("ProviderA", "ModelX");
Console.WriteLine($"{selection.Provider} - {selection.Name}");

// Create a modified copy
var updated = selection with { Name = "ModelY" };
```

## Notes
- Properties are init-only; instances are immutable after creation.
- Use with-expressions for creating a modified copy without mutating the original.
- JSON serialization uses the property names Provider and Name by default; customize with serialization attributes if needed.


---

## SetActiveModelRequest
> **File:** `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`  
> **Kind:** record

```csharp
public sealed record SetActiveModelRequest(string? Provider, string? Name)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Provider` | `string?` | — |
| [`Name`](../../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string?` | — |


SetActiveModelRequest is the API surface for selecting the active model via PUT /api/models/active. It carries an optional Provider and Name; specify both to activate a particular model, or pass null for both to clear the preference and fall back to the config default.

## Remarks
This symbol exists as a lightweight DTO that encodes the client's intent to select or reset the active model. Representing the provider/name pair as a single immutable record makes the contract easy to reason about and ensures value-based equality for caching or testing scenarios. It sits at the boundary between HTTP payloads and domain configuration, enabling callers to override defaults in a controlled, explicit way.

## Example
```csharp
// Typical usage: specify a provider and model
var request = new SetActiveModelRequest("ProviderX", "ModelY");

// Clear the active model preference and fall back to config default
var clearRequest = new SetActiveModelRequest(null, null);
```

## Notes
- Nullability semantics are explicit for the all-null clear case; partial nulls are not defined by this contract.
- When using a serializer, ensure null values are preserved if you rely on the clear behavior; some serializers omit nulls by default, which can prevent the server from recognizing the clear action.


---