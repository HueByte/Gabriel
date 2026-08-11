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


AvailableModel is an immutable record that encapsulates a single model available from any registered provider in the UI's models dropdown. It is constructed by flattening each provider's Models via IModelCatalog to produce a unified, cross-provider view. The entry carries identity (Provider, Name) and operational characteristics (ContextWindowTokens and the optional CompactThreshold), pricing metadata (InputPricePerMTokens, OutputPricePerMTokens, CacheReadPricePerMTokens, CacheWritePricePerMTokens), a bootstrap indicator (IsDefault) that marks the catalog-wide default, and a ToolMode describing how tooling should interact with the model.

## Remarks
AvailableModel plays the role of a catalog-agnostic descriptor for a model. It lets the UI present a unified dropdown across providers without depending on provider-specific types, while the immutable record ensures thread-safety and predictable sharing across components. The pricing fields and the context/window-related fields keep policy decisions centralized, so callers can estimate cost and capability without querying individual providers. The IsDefault flag marks the bootstrap choice for the entire catalog, making it straightforward to bootstrap the initial user experience.

## Notes
- CompactThreshold is nullable; check HasValue before using it.
- Prices use decimal to preserve precision for token-cost calculations.
- IsDefault is catalog-scoped; ensure a single entry is designated as the bootstrap default to avoid startup ambiguity.