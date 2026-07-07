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


Represents the per-turn model selection used by the system. This sealed, immutable record bundles the provider name, the wire-level model identifier, the context window size in tokens, an optional per-model compact threshold override, and the tool-handling mode. It is produced by IModelCatalog.Resolve from a user’s PreferredProvider / PreferredModel (with a config-driven fallback) and threaded through the agent loop so the provider invocation, the compact heuristic, the metrics endpoint, and the tool-emulation wrapper all agree on which model is in play.

## Remarks
This record acts as a single source of truth for the active model across the runtime, coordinating provider invocation, compact heuristics, metrics reporting, and tool emulation. By encapsulating provider, model name, context window, and mode in one immutable value object, the system can reason about identity, caching, and reproducibility without scattering literal strings around.

## Notes
- CompactThreshold being null means no per-model override; global defaults apply.
- ContextWindowTokens should be positive; invalid values indicate misconfiguration.
- Ensure Name belongs to the specified Provider; a mismatch can lead to confusing behavior.