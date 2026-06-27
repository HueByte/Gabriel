# ModelSelection

> **File:** `src/api/Gabriel.Core/Configuration/ModelSelection.cs`  
> **Kind:** record

Represents the resolved, per-turn model choice used across the agent loop and provider integrations. Use this immutable snapshot when you need a single, authoritative descriptor of which provider/model is in play for a turn — it ensures the provider call, compacting heuristics, metrics, and any tool-emulation wrapper all agree on the same model settings.

## Remarks
This record is produced by IModelCatalog.Resolve from a user's PreferredProvider / PreferredModel (with configuration-driven fallbacks) and then threaded through the agent loop. It centralizes the provider name, the wire-level model identifier, the context window size (in tokens), an optional compact-mode threshold, and the tool-handling mode so separate components don't diverge on which model or behavior should be used for a given turn.

## Example
```csharp
// Construct a snapshot of the selected model for a single turn. Replace
// ToolMode.Default with the appropriate enum value defined in your codebase.
var selection = new ModelSelection(
    Provider: "openai",
    Name: "gpt-4o",
    ContextWindowTokens: 8192,
    CompactThreshold: 0.5,
    ToolMode: ToolMode.Default
);

// Pass this immutable selection to any component that needs to act
// consistently for the turn (provider client, compacting logic, metrics, etc.).
// e.g. providerClient.InvokeModelAsync(selection, input);
```

## Notes
- CompactThreshold is nullable: a null value means there is no per-model compact override and consumers should fall back to the system/default compacting behavior.
- ModelSelection is an immutable record — it represents a snapshot for a single turn; modify selection by creating a new instance rather than mutating.
- ContextWindowTokens should reflect the actual token limit expected by the provider/model; callers may need to validate or clamp this value before sending payloads to the provider.