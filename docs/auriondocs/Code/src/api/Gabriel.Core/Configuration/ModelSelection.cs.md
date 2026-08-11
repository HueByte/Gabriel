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


ModelSelection is an immutable record that encapsulates the active model context for a workflow. It carries the provider name, model identifier, contextual token budget, an optional compact-override threshold, and the tool-handling mode, enabling downstream components to operate with a consistent, shared understanding of which model is in use.

It is produced by IModelCatalog.Resolve from the user's preferred provider and model (or a configuration-driven fallback) and threaded through the agent loop so that provider calls, the compact heuristic, the metrics endpoint, and the tool-emulation wrapper all agree on the active model.

## Remarks
ModelSelection provides a clear boundary between decision and execution. By bundling all aspects of the chosen model into a single immutable value, it reduces drift between components and simplifies reasoning about which model is active. The record nature of the type also supports safe sharing across threads in concurrent agent workflows.

## Example
```csharp
IModelCatalog catalog = GetCatalog();
ModelSelection selection = catalog.Resolve("OpenAI", "gpt-4-32k");
```

## Notes
- CompactThreshold is nullable; null indicates no explicit override and defers to the system's default heuristic.
- ContextWindowTokens conveys the per-model token budget for the active model and should be interpreted as the maximum tokens available for the model's context, not a strict usage limit.
- ToolMode influences how the tool-emulation wrapper behaves; mismatches between components can lead to inconsistent tool usage unless the same ModelSelection instance is propagated everywhere.