namespace Gabriel.Core.Configuration;

// How a model handles tool / function calls.
//
// Set per-model in LLMModel config so a provider can serve a mixed catalog
// (e.g. a hosted model with native function calling alongside a local
// llama.cpp model that needs emulation). AgentService and the provider
// resolution code branch on this enum to pick the right transport without
// the agent loop knowing the difference.
public enum ToolMode
{
    // Provider supports the standard OpenAI/xAI-style structured tool_calls
    // protocol natively. Tools ride in the sibling "tools" field of the chat
    // call; the provider streams ToolCallReadyEvent for each parsed call.
    // Default for everything we currently target (Grok, OpenAI, Anthropic).
    Native = 0,

    // Provider returns plain text only. Tools are injected as a system-prompt
    // block; the model emits <tool_call>{...}</tool_call> markers inline with
    // its text; GabrielToolBridge parses them out and re-synthesises the
    // native event shape so the agent loop stays uniform.
    Emulated = 1,

    // Model has no tool capability at all - neither native nor emulated.
    // AgentService skips loading tool descriptors entirely; the model picker
    // should communicate this so users know tool-dependent features (memory
    // saves, project file reads, web search) won't work in this conversation.
    None = 2,
}
