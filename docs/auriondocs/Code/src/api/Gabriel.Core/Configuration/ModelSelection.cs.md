# ModelSelection

> **File:** `src/api/Gabriel.Core/Configuration/ModelSelection.cs`  
> **Kind:** record

```csharp
public sealed record ModelSelection(
    string Provider,
    string Name,
    int ContextWindowTokens,
    double? CompactThreshold,
    ToolMode ToolMode)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Provider` | `string` | — |
| [`Name`](../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `ContextWindowTokens` | `int` | — |
| `CompactThreshold` | `double?` | — |
| [`ToolMode`](ToolMode.cs.md) | [`ToolMode`](ToolMode.cs.md) | — |


Represents the active model selection for a single invocation, encapsulating the provider name, model name, context window size in tokens, an optional compact-threshold override, and the tool-handling mode. This aggregate is produced by IModelCatalog.Resolve from a user-supplied PreferredProvider/PreferredModel (with a config-driven fallback) and is threaded through the agent loop so that the provider call, the compact heuristic, the metrics endpoint, and the tool-emulation wrapper all agree on which model is in play.

## Remarks
By freezing these five values in a single record, the system achieves a stable identity for the model used in a turn, decoupling model selection from downstream usage. This centralization simplifies testing, observability, and potential runtime overrides, since all parties reference the same, canonical model identity.

## Notes
- CompactThreshold is nullable; when null, the default compact-heuristic threshold applies.
- ToolMode governs how tooling emulation is wired; downstream components should gracefully handle all valid ToolMode values.