# ToolMode

> **File:** `src/api/Gabriel.Core/Configuration/ToolMode.cs`  
> **Kind:** enum

```csharp
public enum ToolMode
{

    Native = 0,

    Emulated = 1,

    None = 2,
}
```


ToolMode defines how a model's tool calls are transported and processed at runtime, enabling per-model configuration to support mixed-tool catalogs. Choose Native for standard structured tool_calls, Emulated when the provider returns plain text with embedded tool_call markers, or None to disable tool functionality.

## Remarks
This abstraction decouples the agent loop from transport details, allowing per-model tool handling to vary by deployment. It enables mixed catalogs by choosing a transport tailored to each model or provider capability, while preserving a uniform internal event flow. In short, ToolMode centralizes how tool calls are delivered and consumed.

## Example
```csharp
// Typical configuration examples
var native = new LLMModelConfig { ToolMode = ToolMode.Native };

// Emulated marker-based tool calls
var emulated = new LLMModelConfig { ToolMode = ToolMode.Emulated };

// Tools disabled for a model
var none = new LLMModelConfig { ToolMode = ToolMode.None };
```

## Notes
- In Emulated mode, ensure GabrielToolBridge is present to translate inline <tool_call> markers into the internal event format; otherwise tool invocations will be ignored.
- If ToolMode is changed at runtime or after a session has started, downstream state may become inconsistent; configure per-model ahead of time.