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


Represents a single model entry returned by the /api/models endpoint. It wires together the model’s identity (Provider, Name), its token-context window, per-model pricing (input, output, and cache operations), and flags marking default or selected status. This immutable DTO is used by API clients to render a catalog of available models and to estimate costs for token workloads, while allowing per-model overrides of global settings such as the CompactThreshold. If CompactThreshold is null, the global AgentOptions.CompactThreshold is used, enabling a per-model override alongside a shared default.

## Remarks
ModelDto exists to separate the concerns of data transport and pricing logic. It captures pricing and metadata in a single, serializable record, simplifying UI and service boundaries. The CompactThreshold being nullable encodes the rule "use model-specific threshold when provided, otherwise fall back to the global setting defined in AgentOptions." This design supports tiered pricing and gradual rollouts without duplicating global configuration across all models.

## Example
```csharp
var example = new ModelDto(
  Provider: "OpenAI",
  Name: "gpt-4o",
  ContextWindowTokens: 8192,
  CompactThreshold: 0.18,
  InputPricePerMTokens: 0.06m,
  OutputPricePerMTokens: 0.12m,
  CacheReadPricePerMTokens: 0.0m,
  CacheWritePricePerMTokens: 0.0m,
  IsDefault: true,
  IsSelected: false
);
```

## Notes
- If CompactThreshold is null, the global threshold from AgentOptions applies; callers should be prepared to resolve the effective value at runtime.
- Cache pricing fields may be zero for providers that do not expose caching pricing; treat zero as no cache pricing rather than a negative or special-case value.

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


This record models the response payload for the GET /api/models endpoint. It carries two related pieces of state: AvailableModels, the catalog of ModelDto entries exposed by the system, and Selected, the model currently in use for the user (falling back to the configured default if the user hasn't explicitly chosen one). Consumers rely on this payload to render model-selection UI and to determine the active model for the current session.

## Remarks
This abstraction groups catalog data and the user's choice into a single, immutable payload, reducing round trips and aligning the client view with the server's current state. By representing the payload as a record, you gain value semantics, straightforward equality, and safe, serializable data transfer. The dependency on ModelDto ties the shape of the available options to a shared contract across the API surface, ensuring consistent presentation and validation.

## Notes
- Because AvailableModels and Selected are non-nullable, API responses must include both properties; missing fields can cause deserialization issues in clients using non-nullable reference types.

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


SelectedModelDto represents a user’s selected model by pairing its Provider with the model name. It is an immutable, value-based data carrier used to transport the chosen model through layers (for example from UI input to application services) without scattering two separate strings.

## Remarks
SelectedModelDto leverages a record to provide value-based equality and immutability for a small data carrier. The sealed modifier signals that this is a simple DTO with no inheritance concerns, and the positional constructor makes Provider and Name required at creation. Its shape makes deconstruction and with-expressions straightforward, enabling clean copies when only one field needs change.

## Example
```csharp
var selection = new SelectedModelDto("OpenAI", "GPT-4");
var revised = selection with { Name = "GPT-4 Turbo" };
```

## Notes
- Nullability: If your project enables nullable reference types, Provider and Name are non-nullable; ensure you supply non-null values or adjust types accordingly.
- This is a pure data carrier intended for transport; avoid placing business logic or validation inside the type itself and perform such concerns at boundary or service layers.

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


Represents the payload used to specify the active model for the API. It carries two optional fields: Provider and Name. When you PUT to /api/models/active, you can supply these fields to select a model; if both are null, the API clears the explicit preference and falls back to the configured default.

## Remarks
This small, immutable data transfer object encapsulates the concept of selecting an active model from a provider-name pair or clearing the choice entirely. Using a value-based record makes API calls self-describing and straightforward to reason about in tests and client code; it also aligns with other DTOs in the API surface by expressing intent through a single, well-scoped type.

## Example
```csharp
// Clear the active model preference (fallback to config default)
var clear = new SetActiveModelRequest(null, null);

// Set an explicit active model
var setActive = new SetActiveModelRequest("OpenAI", "GPT-4");
```


---