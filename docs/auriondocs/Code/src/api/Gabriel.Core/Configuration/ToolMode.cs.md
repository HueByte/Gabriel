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


ToolMode controls how tool calls are delivered from the model to the agent/provider. It is configured per-model to enable a mixed catalog of backends and to let the system select the correct transport without exposing the underlying mechanism to the agent loop. The three values map to: Native—OpenAI/xAI-style structured tool_calls; Emulated—text with inline <tool_call> markers parsed into native events; None—no tool capability.

## Remarks
By centralizing these transport strategies into a single enum, the architecture can support diverse providers while keeping the agent loop uniform. It communicates at runtime which tooling capabilities are available and isolates transport concerns from high-level behavior. Changes to ToolMode affect the transport path rather than how the model is invoked.

## Example
```csharp
var mode = ToolMode.Native;
switch (mode)
{
  case ToolMode.Native:
    // use native tool_calls path
    break;
  case ToolMode.Emulated:
    // use emulated path
    break;
  case ToolMode.None:
    // disable tool usage
    break;
}
```

## Notes
- Native vs Emulated requires corresponding support on the provider side.
- Emulated mode relies on the GabrielToolBridge to reconstruct native events; without the bridge, tooling won't execute.
- None disables tool usage; ensure UI/flows reflect that to avoid user confusion.