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


ToolMode indicates how a model backend handles tool invocation during a chat session. It is configured per-model so a provider can service a mixed catalog (for example, a hosted model with native function calling alongside a local model that requires emulation). The agent service and provider resolution code branch on this value to select the correct transport without the agent loop needing to know the underlying difference.

## Remarks
ToolMode isolates transport concerns from the rest of the tool invocation path. It lets the hosting environment accommodate providers that natively emit OpenAI/xAI-style tool_calls, those that embed tool calls as plain text and require a bridge to synthesize the events, and those that expose no tool capability at all. This separation enables a mixed-catalog deployment where the same agent loop can operate with different backends without special-casing each capability.

## Notes
- Native mode requires that the provider streams ToolCallReadyEvent for each parsed call; the agent loop expects to receive them in a uniform channel as parsed tool calls.
- Emulated mode injects tools via a system-prompt block; the model emits <tool_call>{...}</tool_call> markers inline, and GabrielToolBridge reconstructs the native event shape so the agent loop remains uniform.
- None disables tool capability; UI and configuration should reflect this so tool-dependent features (memory saves, project file reads, web search) won't be attempted in the conversation.