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


ModelDto encapsulates the data for one entry returned by the /api/models endpoint, including per-model pricing and context/compact settings. It is typically consumed when listing available models and calculating the cost of using a particular model.

## Remarks
ModelDto is an immutable record used as a wire-format contract. The CompactThreshold field is nullable to allow per-model overrides of the global compact threshold; when null, the consumer should fallback to AgentOptions.CompactThreshold. Pricing values are stated per-million tokens, and cache-related prices are provided only when the provider exposes them; otherwise, cache price fields are zero. The IsDefault and IsSelected flags help convey UI defaults and selection state without altering the pricing data.

## Example
```csharp
// Example usage: build a DTO for a hypothetical model
var example = new ModelDto(
    Provider: "OpenAI",
    Name: "gpt-4-turbo",
    ContextWindowTokens: 128000,
    CompactThreshold: null,
    InputPricePerMTokens: 0.003m,
    OutputPricePerMTokens: 0.003m,
    CacheReadPricePerMTokens: 0.001m,
    CacheWritePricePerMTokens: 0.001m,
    IsDefault: false,
    IsSelected: true
);
```

## Notes
- Null CompactThreshold means the model uses the global compact threshold from AgentOptions; ensure you have that value available when computing behavior.
- Prices are per-million tokens; convert or aggregate tokens accordingly when calculating total cost. If a provider does not expose caching pricing, the CacheRead/Write fields will be zero, reflecting that limitation.


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


ModelsResponse represents the payload returned by GET /api/models. It carries two pieces of information: AvailableModels, a list of all models exposed by the catalog, and Selected, the model the agent will currently use for the current user (defaulting to the configured value if the user hasn’t made a choice yet). The record is sealed and immutable, making it a stable, serializable snapshot suitable for API responses.

## Remarks

This type serves as a simple, client-facing contract that separates the catalog of existing models from the user-specific active selection. By modeling the payload as a value-based record, it guarantees deterministic equality and immutability, reducing accidental mutations after creation. It collaborates with ModelDto to describe each model and with SelectedModelDto to convey the current selection; together they provide a concise, server-friendly payload for clients to render available options and the active choice.

## Notes
- The AvailableModels collection is exposed as IReadOnlyList to emphasize immutability; clients should treat it as a read-only snapshot and avoid mutating the collection.
- Selected reflects the agent’s current choice for the user; if no explicit selection exists, it corresponds to the default configuration.

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


SelectedModelDto is an immutable data carrier that captures the identity of a model chosen from a provider. Implemented as a sealed record with two positional parameters, Provider and Name, it enables value-based equality, deconstruction, and straightforward serialization when transporting the selection across boundaries such as APIs or UI layers without embedding behavior.

## Remarks
This symbol serves as a boundary-facing contract for model selection. It decouples the notion of which model was chosen from domain logic, enabling consistent transfer and comparison across layers and serialization formats while remaining a simple, immutable data container.

## Example
```csharp
var selected = new SelectedModelDto("OpenAI", "GPT-4");
Console.WriteLine($"{selected.Provider}: {selected.Name}"); // OpenAI: GPT-4

// Deconstruction is available thanks to positional parameters
var (provider, modelName) = selected;
Console.WriteLine($"{provider} -> {modelName}"); // OpenAI -> GPT-4
```


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


SetActiveModelRequest is an immutable data transfer object used as the request body for the PUT /api/models/active endpoint. It carries optional information to select an active model by provider and name, or to clear the current preference and revert to the configuration default when both fields are null.

## Remarks
This record is sealed and uses positional parameters, making it a simple, value-based carrier of two optional strings. By exposing both Provider and Name as nullable, the API can express either a targeted model selection or a reset to defaults without introducing additional mutation methods. The immutable, value-like nature of a record helps ensure equality semantics and thread-safety when used as part of request payloads.

## Example
```csharp
// Activate a specific model from a provider
var request = new SetActiveModelRequest("OpenAI", "gpt-4");

// Clear the active model preference and fall back to config default
var reset = new SetActiveModelRequest(null, null);
```

## Notes
- To clear the active model preference, pass null for both fields. Partial nulls are documented as valid inputs but their server-side interpretation depends on the API implementation.


---