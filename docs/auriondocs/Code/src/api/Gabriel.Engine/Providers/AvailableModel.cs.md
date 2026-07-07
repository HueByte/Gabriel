# AvailableModel

> **File:** `src/api/Gabriel.Engine/Providers/AvailableModel.cs`  
> **Kind:** record

```csharp
public sealed record AvailableModel(
    string Provider,
    string Name,
    int ContextWindowTokens,
    double? CompactThreshold,
    decimal InputPricePerMTokens,
    decimal OutputPricePerMTokens,
    decimal CacheReadPricePerMTokens,
    decimal CacheWritePricePerMTokens,
    bool IsDefault,
    ToolMode ToolMode)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Provider` | `string` | — |
| [`Name`](ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `ContextWindowTokens` | `int` | — |
| `CompactThreshold` | `double?` | — |
| `InputPricePerMTokens` | `decimal` | — |
| `OutputPricePerMTokens` | `decimal` | — |
| `CacheReadPricePerMTokens` | `decimal` | — |
| `CacheWritePricePerMTokens` | `decimal` | — |
| `IsDefault` | `bool` | — |
| [`ToolMode`](../../Gabriel.Core/Configuration/ToolMode.cs.md) | [`ToolMode`](../../Gabriel.Core/Configuration/ToolMode.cs.md) | — |


Represents a single model option that the user can select in the UI dropdown. The UI builds a flattened list by iterating across all registered IChatProvider instances' Models via IModelCatalog, so AvailableModel provides a uniform view of each provider's model options. The IsDefault flag marks the config-declared bootstrap choice; there should be a single default across the entire catalog.

## Remarks
This sealed record serves as a lightweight, immutable data transfer object used by the UI and catalog layers. Its value-based equality makes it a natural key for dropdown items, and its decoupled shape keeps provider-specific details out of the UI. By aggregating across providers, it enables a consistent selection experience regardless of the underlying model provider.

## Notes
- If multiple AvailableModel entries have IsDefault set to true, bootstrap selection may become ambiguous; enforce a single default in configuration or central catalog logic.