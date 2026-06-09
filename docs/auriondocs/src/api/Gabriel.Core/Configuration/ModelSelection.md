# ModelSelection

> **File:** `src/api/Gabriel.Core/Configuration/ModelSelection.cs`  
> **Kind:** record

Represents a resolved, per-turn model choice: the provider identifier, the provider-specific model name, the effective context window in tokens, an optional per-model compactness threshold, and how the model handles tool/function calls. A ModelSelection is produced by IModelCatalog.Resolve (from a user's preferred provider/model with config-driven fallbacks) and is threaded through the agent loop so the provider transport, compacting heuristic, metrics, and tool-emulation all operate on the same concrete model selection.

## Remarks
ModelSelection centralizes the runtime decisions that must remain consistent across several subsystems (agent execution, provider calls, metrics, and tool handling). By carrying both the wire-level identifiers (Provider, Name) and runtime controls (ContextWindowTokens, CompactThreshold, ToolMode), it avoids mismatches where different components might otherwise infer different models or capabilities.

## Example
```csharp
// Create a concrete selection for a hosted provider with native tool support
var selection = new ModelSelection(
    Provider: "openai",
    Name: "gpt-4o-mini",
    ContextWindowTokens: 8192,
    CompactThreshold: 0.75,
    ToolMode: ToolMode.Native);

// Use values downstream
if (selection.ToolMode == ToolMode.Emulated)
{
    // enable tool-emulation wrapper
}

// To change one value, create a new selection (records are immutable)
var biggerWindow = selection with { ContextWindowTokens = 32768 };
```

## Notes
- CompactThreshold is nullable: null means "use the model's configured default" rather than an explicit override.
- Provider and Name are wire-level identifiers and must match the provider's catalog; they are not user-facing display names.
- ModelSelection is an immutable record; modify any field by creating a new instance (the C# record 'with' expression is appropriate).