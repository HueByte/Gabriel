# AvailableModel

> **File:** `src/api/Gabriel.Engine/Providers/AvailableModel.cs`  
> **Kind:** record

Represents a single model option surfaced to consumers (for example, a UI dropdown entry). This record is a provider-agnostic, flattened view of models produced by the IModelCatalog so callers can show model metadata, pricing and selection state without touching provider-specific types.

## Remarks
IModelCatalog constructs instances of this record by iterating every registered IChatProvider and collecting their declared models; the result is a single unified list across all providers. The IsDefault flag identifies the one model chosen by configuration as the bootstrap/default model. Pricing fields are stored as decimals and express provider-reported cost per "M tokens" billing unit; consult the provider or configuration for the exact currency and billing granularity. ToolMode indicates how the model is intended to be used (tool/assistant/completion semantics) and guides client-side behavior and UI affordances.

## Example
```csharp
// Build an AvailableModel for display in a dropdown and pick the default
var model = new AvailableModel(
    Provider: "openai",
    Name: "gpt-4o-mini",
    ContextWindowTokens: 8192,
    CompactThreshold: 0.75,
    InputPricePerMTokens: 0.0030m,
    OutputPricePerMTokens: 0.0040m,
    CacheReadPricePerMTokens: 0.0001m,
    CacheWritePricePerMTokens: 0.0002m,
    IsDefault: false,
    ToolMode: ToolMode.Chat
);

// Typical use: show models in UI and highlight the default
IEnumerable<AvailableModel> allModels = modelCatalog.GetAll();
var defaultModel = allModels.FirstOrDefault(m => m.IsDefault);
```

## Notes
- The record is immutable (C# record positional syntax); copy-and-update can be done with the with-expression.
- Pricing fields represent provider-reported cost per "M tokens" unit; do not assume currency or billing period — read provider config for details.
- Provider + Name together identify the model entry across the catalog; IsDefault is expected to be set on at most one entry in the aggregated list.