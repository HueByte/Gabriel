# ToolMode

> **File:** `src/api/Gabriel.Core/Configuration/ToolMode.cs`  
> **Kind:** enum

```csharp
// How a model handles tool / function calls.
//
// Set per-model in LLMModel config so a provider can serve a mixed catalog
// (e.g. a hosted model with native function calling alongside a local
// llama.cpp model that needs emulation). AgentService and the provider
// resolution code branch on this enum to pick the right transport without
// the agent loop knowing the difference.
public enum ToolMode
```


Specifies how a model handles tool / function calls and is set per-model (e.g., in an LLMModel configuration). Use this enum when configuring a model or when provider/AgentService resolution needs to choose the appropriate transport and call-handling behavior (native structured calls, emulated inline markers, or no tool capability).

## Remarks
This enum exists to support mixed model catalogs and keep the agent loop implementation uniform. Providers and AgentService branch on ToolMode to decide whether to use a native tool-calling transport, parse emulated inline markers, or skip tool integration entirely; that allows hosted models that natively support structured tool_calls to coexist with local or legacy models that require emulation.

## Example
```csharp
// Configure a model to use native tool calls
var config = new LLMModelConfig
{
    Name = "example-model",
    ToolMode = ToolMode.Native,
    // ...other settings
};

// Configure a plain-text model that uses emulated tool markers
var legacyConfig = new LLMModelConfig
{
    Name = "legacy-text-model",
    ToolMode = ToolMode.Emulated,
};

// Configure a model with no tool capability
var simpleConfig = new LLMModelConfig
{
    Name = "read-only-model",
    ToolMode = ToolMode.None,
};
```

## Notes
- Emulated mode relies on a parser/bridge (e.g., GabrielToolBridge) to extract inline <tool_call> markers and re-synthesize native event shapes; enabling Emulated without such wiring means the agent will not receive structured tool call events.
- None disables tool integration: AgentService will skip loading tool descriptors and tool-dependent features (memory saves, file reads, web search, etc.) will not be available for that model.