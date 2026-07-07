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


AvailableModel is a value object that represents a selectable model entry in the UI, aggregating models from all registered providers into a single, flattened catalog. It carries the essential metadata the UI and pricing logic need, including the provider identifier, the model's display name, token context window, optional compacting threshold, per-token pricing, cache access costs, whether this entry is the catalog bootstrap default, and the tool mode to use when invoking the model.

## Remarks

Because the catalog is built by scanning each IChatProvider's Models list and then flattening them into AvailableModel, this record acts as a stable, provider-agnostic contract for the UI. Its immutability and structural equality enable straightforward caching and diffing, while the IsDefault flag centralizes the bootstrap selection across providers. The monetary fields use decimal to preserve precision and avoid floating-point errors in price calculations, and CompactThreshold being nullable communicates that some models do not define a soft limit.

## Notes

- CompactThreshold is nullable; null means no threshold is defined for when to switch to a compact representation.
- Prices are expressed per MTokens; display/formatting should apply currency culture as appropriate.
- IsDefault indicates the bootstrap choice for the catalog; there should be at most one default across the entire catalog, and consumer UI logic may preselect this model when available.