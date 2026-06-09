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

Represents a single entry in the /api/models response: identifies a model (provider + name), its context window size, optional per-model compacting threshold, per‑million‑token pricing for input/output and cache operations, and simple selection flags. Reach for this DTO when listing available models in the UI or calculating estimated cost for a request.

## Remarks
This record is the wire shape used by the API to convey model capabilities and cost information to clients. Pricing fields are specified as amounts per million tokens (useful for consistent cost calculations across providers). The CompactThreshold is nullable: when null the caller should fall back to the global AgentOptions.CompactThreshold; a non-null value (for example 0.18) means the model requests compaction when the remaining context reaches that fraction of the model's context window. CacheReadPricePerMTokens and CacheWritePricePerMTokens remain 0 for providers that do not expose caching pricing.

## Example
```csharp
// Model with provider-specific compact threshold and cache pricing
var modelA = new ModelDto(
    Provider: "acme-ai",
    Name: "acme-gpt-4-small",
    ContextWindowTokens: 8192,
    CompactThreshold: 0.18,              // trigger compaction at 18% of window
    InputPricePerMTokens: 3.50m,         // $3.50 per 1,000,000 input tokens
    OutputPricePerMTokens: 6.00m,
    CacheReadPricePerMTokens: 0.10m,
    CacheWritePricePerMTokens: 0.05m,
    IsDefault: false,
    IsSelected: true
);

// Model that uses the global compact threshold and no cache pricing advertised
var modelB = new ModelDto(
    Provider: "other-ai",
    Name: "other-embedder",
    ContextWindowTokens: 2048,
    CompactThreshold: null,              // use AgentOptions.CompactThreshold
    InputPricePerMTokens: 0.25m,
    OutputPricePerMTokens: 0.0m,
    CacheReadPricePerMTokens: 0.0m,      // provider didn't expose cache pricing
    CacheWritePricePerMTokens: 0.0m,
    IsDefault: true,
    IsSelected: false
);
```

## Notes
- CompactThreshold is nullable: treat null as "defer to AgentOptions.CompactThreshold" rather than 0.0.  
- Prices are expressed per million tokens — convert when computing per-token cost (e.g., cost = pricePerMTokens * tokens / 1_000_000).  
- Cache pricing fields may be zero when a provider does not report caching fees; do not assume zero means free caching behavior without verifying provider semantics.

---

## ModelsResponse

> **File:** `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`  
> **Kind:** record

Represents the payload returned by GET /api/models: a catalog of all models the service exposes and the single model the agent will currently use for the requesting user. Use this record when returning both the available model catalog and the user's (or system) selected model in one response.

## Remarks
This lightweight immutable DTO groups two related pieces of information to avoid multiple round-trips: AvailableModels lists everything in the catalog, while Selected indicates which model the agent should use for this user (it will reflect a user choice or fall back to the configured default). The record shape makes it convenient to produce and serialize directly from controller actions.

## Example
```csharp
var available = new List<ModelDto> {
    new ModelDto("gpt-4", "GPT-4", "Large general-purpose model"),
    new ModelDto("gpt-4o", "GPT-4o", "Optimized for latency")
};
var selected = new SelectedModelDto("gpt-4");
var response = new ModelsResponse(available, selected);
// return Ok(response); // in an ASP.NET Core controller
```

## Notes
- AvailableModels is exposed as an IReadOnlyList to signal consumers should not attempt to modify the collection.
- Selected is authoritative for what the agent will use; it may represent a configured default if the user has not made an explicit choice.

---

## SelectedModelDto

> **File:** `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`  
> **Kind:** record

Represents a selected model by its provider and model name. Use this DTO in API contracts or application boundaries when you need to pass or store the combination of a model provider (for example, "openai") and the provider-specific model identifier (for example, "gpt-4").

## Remarks
This is a compact, immutable positional record intended for data transfer. It provides value-based equality, deconstruction, and a concise ToString implementation out of the box. The type is sealed to communicate that it is a simple data carrier and is not designed for inheritance or behavior extension.

## Example
```csharp
// Create an instance
var selected = new SelectedModelDto("openai", "gpt-4");

// Deconstruct
var (provider, name) = selected;

// Value equality
var same = new SelectedModelDto("openai", "gpt-4");
Console.WriteLine(selected == same); // True

// Create a modified copy using 'with'
var updated = selected with { Name = "gpt-4o" };
```

## Notes
- Properties are init-only and the record is immutable; prefer creating a new instance (with-expression) to represent changes.
- Equality and hashing are based on the Provider and Name values (value semantics).
- The positional parameter order determines deconstruction and the generated ToString output.

---

## SetActiveModelRequest

> **File:** `src/api/Gabriel.API/Contracts/Models/ModelDto.cs`  
> **Kind:** record

Represents the request body for the PUT /api/models/active endpoint. Use this DTO to set the active model by specifying its provider and name; to clear the explicit preference and revert to the configured default, pass both Provider and Name as null.

## Remarks
This immutable record is an API contract used to convey the selected model (provider + model name) from a client to the server. It exists to separate transport-level model selection from any internal configuration or defaults the server may maintain.

## Example
```csharp
// Set the active model
var setReq = new SetActiveModelRequest("openai", "gpt-4");
await httpClient.PutAsJsonAsync("/api/models/active", setReq);

// Clear the explicit preference and fall back to the server configuraton default
var clearReq = new SetActiveModelRequest(null, null);
await httpClient.PutAsJsonAsync("/api/models/active", clearReq);
```

## Notes
- Passing Provider and Name both as null signals the server to clear the stored preference and use the configured default.
- Both properties are nullable strings; the server determines how to treat partially null values (e.g., Provider non-null but Name null).

---