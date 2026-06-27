# ToolMode

> **File:** `src/api/Gabriel.Core/Configuration/ToolMode.cs`  
> **Kind:** enum

Indicates how a model handles tool/function calls so the agent runtime and provider resolution can pick the appropriate transport and integration strategy. Set per-model in the LLMModel configuration to tell AgentService whether the provider supports structured tool calls natively, needs emulation via inlined markers, or has no tool capability at all.

## Remarks
This enum exists to support heterogeneous model catalogs where different backends expose tool-calling in different ways (for example, hosted models that support the OpenAI-style tool_calls protocol versus local models that only emit text). AgentService and the provider-selection logic branch on this value to load descriptors, choose how to stream parsed calls, and perform any emulation (e.g., GabrielToolBridge) so the agent loop remains consistent regardless of the underlying model.

## Example
```csharp
// Configure a model to use native tool calling
var modelConfig = new LLMModelConfig { Name = "gpt-x", ToolMode = ToolMode.Native };

// Consumer code branching behavior
switch (modelConfig.ToolMode)
{
    case ToolMode.Native:
        // subscribe to native ToolCallReadyEvent stream
        break;
    case ToolMode.Emulated:
        // inject tools into system prompt and parse <tool_call> markers
        break;
    case ToolMode.None:
        // skip loading tool descriptors; disable tool-dependent features
        break;
}
```

## Notes
- Default value is Native (0); an unspecified enum field will be treated as Native unless explicitly set.
- Emulated mode relies on text markers and a parser/bridge (e.g., GabrielToolBridge) to reconstruct the native event shape; ensure the bridge is available if you choose Emulated.
- None tells the agent runtime not to load tool descriptors; features that depend on tools (memory saves, file access, web search) will be unavailable for that model.