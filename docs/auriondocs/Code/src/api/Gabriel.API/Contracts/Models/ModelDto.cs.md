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

Represents the wire-format metadata for a single model returned by the /api/models endpoint. Contains identification (provider and model name), the model's context window size, optional per-model compaction threshold, pricing expressed per 1,000,000 tokens for input/output and cache operations, and two boolean flags intended for client UI (default and currently selected model).

## Remarks
This DTO is designed for use by API responses and client UIs that need to display model choices and compute cost estimates. The nullable CompactThreshold allows a model to override the global AgentOptions.CompactThreshold when a model requires a different rolling-compact trigger (for example, to account for tiered pricing). Cache read/write prices are kept at 0 when a provider does not expose caching pricing information.

## Example
```csharp
// Example: model with no per-model compact override
var modelA = new ModelDto(
    Provider: "openai",
    Name: "gpt-4o",
    ContextWindowTokens: 8192,
    CompactThreshold: null,              // use global AgentOptions.CompactThreshold
    InputPricePerMTokens: 0.12m,
    OutputPricePerMTokens: 0.18m,
    CacheReadPricePerMTokens: 0m,        // provider doesn't charge for cache reads (or not exposed)
    CacheWritePricePerMTokens: 0.01m,
    IsDefault: true,
    IsSelected: false
);

// Example: model with per-model compact override (trigger at 18% of context window)
var modelB = new ModelDto(
    Provider: "acme",
    Name: "acme-large",
    ContextWindowTokens: 32768,
    CompactThreshold: 0.18,              // trigger compaction at 18% of the context window
    InputPricePerMTokens: 0.05m,
    OutputPricePerMTokens: 0.05m,
    CacheReadPricePerMTokens: 0m,
    CacheWritePricePerMTokens: 0m,
    IsDefault: false,
    IsSelected: true
);
```

## Notes
- CompactThreshold is a fraction (e.g., 0.18 means 18% of the context window). A null value means the system should fall back to the global AgentOptions.CompactThreshold.
- All price fields are expressed as cost per 1,000,000 tokens (per-million tokens) and use decimal to avoid floating-point rounding issues.
- CacheReadPricePerMTokens and CacheWritePricePerMTokens may be zero when a provider does not expose caching pricing; treat zero as "no charge or not provided."

---

## ModelsResponse

> **File:** `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`  
> **Kind:** record

A DTO returned by GET /api/models that lists every model the service catalog exposes and indicates which model the agent will currently use for the requesting user. Use this record when responding to clients that need both the full catalog of available models and the currently selected (or default) model for that user.

## Remarks
This record bundles two concerns that clients commonly need together: the complete set of available models (the catalog) and the single model that the agent will actually use for the user. The Selected property echoes the agent's current choice (it will be the configured default if the user has not made a selection), while AvailableModels is the authoritative list of what the catalog exposes.

## Example
```csharp
// Prepare data (ModelDto and SelectedModelDto instances assumed available)
IReadOnlyList<ModelDto> available = new List<ModelDto> { modelDtoA, modelDtoB };
SelectedModelDto selected = userSelectedModel ?? defaultSelectedModel;

var response = new ModelsResponse(available, selected);
// return response from a controller action as the API payload
```

## Notes
- IReadOnlyList is a read-only view but does not guarantee the underlying collection is immutable; pass an immutable collection or a defensive copy if callers must not see later mutations.
- Selected reflects the agent's current choice for that user — it may represent a system/config default when the user has not picked a model.

---

## SelectedModelDto

> **File:** `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`  
> **Kind:** record

Represents a selected model by its provider and model name. Use this lightweight DTO when you need to identify or transfer which provider/model combination is chosen (for example in API requests, responses, or internal contract boundaries).

## Remarks
This is a sealed positional record so the compiler generates immutable properties (Provider and Name), value-based equality, Deconstruct, and support for with-expressions. Being a record makes it convenient for comparisons and creating modified copies without mutating the original object; sealing prevents inheritance and keeps the shape stable for consumers.

## Example
```csharp
// create
var sel = new SelectedModelDto("openai", "gpt-4o");

// deconstruct
var (provider, name) = sel;

// create a modified copy
var other = sel with { Name = "gpt-4o-mini" };
```

## Notes
- Equality is value-based: two instances with identical Provider and Name compare equal.
- Properties are init-only (immutable after construction); use the with-expression to create modified copies.
- If this DTO is serialized to JSON, ensure the serializer's naming policy matches the API contract (property names are "Provider" and "Name" by default).

---

## SetActiveModelRequest

> **File:** `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`  
> **Kind:** record

Represents the request body for the PUT /api/models/active endpoint; used to set a preferred model by provider/name or to clear the preference. Both Provider and Name are nullable — passing null for both clears the stored preference and causes the system to fall back to the configured default.

## Remarks
This is a small immutable DTO used only as the API payload for updating the active model preference. It intentionally allows null values so callers can explicitly clear the preference (null/null) rather than supplying an empty string.

## Example
```csharp
// Set a specific model
var req = new SetActiveModelRequest("openai", "gpt-4");
var json = System.Text.Json.JsonSerializer.Serialize(req);
await httpClient.PutAsync("/api/models/active", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

// Clear the preference (revert to config default)
var clearReq = new SetActiveModelRequest(null, null);
var clearJson = System.Text.Json.JsonSerializer.Serialize(clearReq);
await httpClient.PutAsync("/api/models/active", new StringContent(clearJson, System.Text.Encoding.UTF8, "application/json"));
```

## Notes
- Passing Provider = null and Name = null explicitly clears any stored preference; do not rely on empty strings to have the same effect.
- This record is immutable; use the constructor or the "with" expression to create modified copies if needed.
- When serializing to JSON, serializer settings that omit null values will exclude those properties from the payload — ensure the receiving endpoint treats omitted fields as intended.

---